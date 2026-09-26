using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Verum.BuildingBlocks.Erros;
using Verum.BuildingBlocks.InteligenciaArtificial;
using Verum.Modules.Busca.Contratos.InteligenciaArtificial;

namespace Verum.Modules.Busca.Aplicacao.InteligenciaArtificial;

internal sealed class DescobertaProdutosIaService(IConsultaIa ia, IConfiguration configuration, ILogger<DescobertaProdutosIaService> logger) : IDescobertaProdutosIaService
{
  private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
  {
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    RespectNullableAnnotations = true
  };

  public async Task<DescobertaProdutosIa> ConsultarAsync(string consulta, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(consulta) || consulta.Length > 500)
      throw ErroAplicacaoException.Validacao("Informe uma consulta de produto com até 500 caracteres.", nameof(consulta));

    var pasta = configuration["InteligenciaArtificial:Produtos:PastaContexto"] ?? "Contextos/Busca";

    var caminho = Path.GetFullPath(pasta, AppContext.BaseDirectory);

    // Arquivos locais controlados pela implantação; nunca caminhos enviados pelo usuário.
    var contexto = await File.ReadAllTextAsync(Path.Combine(caminho, "produtos.txt"), cancellationToken);

    using var esquema = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(caminho, "produtos.schema.json"), cancellationToken));

    var entrada = JsonSerializer.Serialize(new { consulta, consultadaEmUtc = DateTimeOffset.UtcNow }, JsonOptions);

    var resposta = await ia.ConsultarAsync(new ConsultaIa(contexto, entrada, "produtos_verum_v1",
      esquema.RootElement.Clone(), PesquisarWeb: true,
      Provedor: configuration["InteligenciaArtificial:Produtos:Provedor"]), cancellationToken);

    EnvelopeProdutos resultado;

    try
    {
      resultado = resposta.Conteudo.Deserialize<EnvelopeProdutos>(JsonOptions)
        ?? throw new JsonException();
    }
    catch (JsonException)
    {
      throw new RespostaIaInvalidaException("PRODUTOS_JSON_INVALIDO");
    }

    ValidarEstrutura(resultado);

    var validos = new List<ProdutoCandidatoIa>();

    var descartados = new List<CandidatoIaDescartado>();

    for (var indice = 0; indice < resultado.Produtos.Length; indice++)
    {
      try
      {
        var produto = resultado.Produtos[indice].Deserialize<ProdutoCandidatoIa>(JsonOptions);

        ValidarCandidato(produto, resposta.Fontes);

        validos.Add(produto!);
      }
      catch (JsonException)
      {
        RegistrarDescarte(indice, "PRODUTO_JSON_INVALIDO");
      }
      catch (RespostaIaInvalidaException erro)
      {
        RegistrarDescarte(indice, erro.Codigo);
      }
    }

    var motivo = validos.Count == 0 && descartados.Count > 0
      ? "Nenhum candidato retornado passou pela validação."
      : resultado.MotivoSemResultado;

    return new DescobertaProdutosIa(new ResultadoProdutosIa
    {
      ConsultaInterpretada = resultado.ConsultaInterpretada,
      MotivoSemResultado = motivo,
      Produtos = validos.ToArray()
    }, resposta) { Descartados = descartados.ToArray() };

    void RegistrarDescarte(int indice, string codigo)
    {
      descartados.Add(new CandidatoIaDescartado(indice, codigo));

      logger.LogWarning("Candidato de IA descartado. Provedor {Provedor}, Resposta {IdResposta}, Índice {Indice}, Código {Codigo}",
        resposta.Provedor, resposta.IdResposta, indice, codigo);
    }
  }

  private static void ValidarEstrutura(EnvelopeProdutos resultado)
  {
    if (string.IsNullOrWhiteSpace(resultado.ConsultaInterpretada) || resultado.ConsultaInterpretada.Length > 500
      || resultado.Produtos is null || resultado.Produtos.Length > 10
      || (resultado.Produtos.Length == 0 && string.IsNullOrWhiteSpace(resultado.MotivoSemResultado))
      || (resultado.Produtos.Length > 0 && resultado.MotivoSemResultado is not null))
      throw new RespostaIaInvalidaException("PRODUTOS_INCONSISTENTES");
  }

  private sealed record EnvelopeProdutos
  {
    public required string ConsultaInterpretada { get; init; }

    public required string? MotivoSemResultado { get; init; }

    public required JsonElement[] Produtos { get; init; }
  }

  private static void ValidarCandidato(ProdutoCandidatoIa? produto, IReadOnlyList<FonteIa> fontes)
  {
    if (produto is null || string.IsNullOrWhiteSpace(produto.Nome) || produto.Nome.Length > 500
      || string.IsNullOrWhiteSpace(produto.Loja) || string.IsNullOrWhiteSpace(produto.Evidencia)
      || !UrlPublica(produto.Url) || (produto.ImagemUrl is not null && !UrlPublica(produto.ImagemUrl))
      || produto.Preco is <= 0 || produto.PrecoPix is <= 0 || produto.PrecoAnterior is <= 0
      || produto.ValorParcela is <= 0 || produto.QuantidadeParcelas is <= 0 or > 120
      || ((produto.QuantidadeParcelas is null) != (produto.ValorParcela is null))
      || (produto.Moeda is not null && produto.Moeda != "BRL")
      || ((produto.Preco is not null || produto.PrecoPix is not null || produto.PrecoAnterior is not null || produto.ValorParcela is not null) && produto.Moeda != "BRL")
      || produto.Disponibilidade is not ("disponivel" or "indisponivel" or "desconhecida")
      || produto.CondicaoProduto is not ("novo" or "usado" or "recondicionado" or "desconhecida")
      || produto.Correspondencia is not ("exata" or "aproximada"))
      throw new RespostaIaInvalidaException("PRODUTO_INVALIDO");

    // Rastreabilidade não significa confirmação de preço: isso pertence ao fluxo de Ofertas.
    if (!fontes.Any(x => Uri.TryCreate(x.Url, UriKind.Absolute, out var fonte)
      && fonte.Equals(new Uri(produto.Url))))
      throw new RespostaIaInvalidaException("PRODUTO_SEM_FONTE");
  }

  private static bool UrlPublica(string? valor) =>
    Uri.TryCreate(valor, UriKind.Absolute, out var uri)
    && uri.Scheme is "http" or "https"
    && uri.HostNameType == UriHostNameType.Dns
    && uri.Host.Contains('.')
    && !uri.IsLoopback
    && string.IsNullOrEmpty(uri.UserInfo);
}
