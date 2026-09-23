using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Verum.BuildingBlocks.Erros;
using Verum.BuildingBlocks.InteligenciaArtificial;
using Verum.Modules.Busca.Contratos.InteligenciaArtificial;

namespace Verum.Modules.Busca.Aplicacao.InteligenciaArtificial;

internal sealed class DescobertaProdutosIaService(IConsultaIa ia, IConfiguration configuration) : IDescobertaProdutosIaService
{
  private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
  {
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    RespectNullableAnnotations = true
  };

  public async Task<DescobertaProdutosIa> ConsultarAsync(string consulta, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(consulta) || consulta.Length > 1000)
      throw ErroAplicacaoException.Validacao("Informe uma consulta de produto com até 1000 caracteres.", nameof(consulta));

    var pasta = configuration["InteligenciaArtificial:Produtos:PastaContexto"] ?? "Contextos/Busca";

    var caminho = Path.GetFullPath(pasta, AppContext.BaseDirectory);

    // Arquivos locais controlados pela implantação; nunca caminhos enviados pelo usuário.
    var contexto = await File.ReadAllTextAsync(Path.Combine(caminho, "produtos.txt"), cancellationToken);

    using var esquema = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(caminho, "produtos.schema.json"), cancellationToken));

    var entrada = JsonSerializer.Serialize(new { consulta, consultadaEmUtc = DateTimeOffset.UtcNow }, JsonOptions);

    var resposta = await ia.ConsultarAsync(new ConsultaIa(contexto, entrada, "produtos_verum_v1",
      esquema.RootElement.Clone(), PesquisarWeb: true,
      Provedor: configuration["InteligenciaArtificial:Produtos:Provedor"]), cancellationToken);

    ResultadoProdutosIa resultado;

    try
    {
      resultado = resposta.Conteudo.Deserialize<ResultadoProdutosIa>(JsonOptions)
        ?? throw new JsonException();
    }
    catch (JsonException)
    {
      throw new RespostaIaInvalidaException("PRODUTOS_JSON_INVALIDO");
    }

    Validar(resultado, resposta.Fontes);

    return new DescobertaProdutosIa(resultado, resposta);
  }

  private static void Validar(ResultadoProdutosIa resultado, IReadOnlyList<FonteIa> fontes)
  {
    if (string.IsNullOrWhiteSpace(resultado.ConsultaInterpretada) || resultado.ConsultaInterpretada.Length > 1000
      || resultado.Produtos is null || resultado.Produtos.Length > 10
      || (resultado.Produtos.Length == 0 && string.IsNullOrWhiteSpace(resultado.MotivoSemResultado))
      || (resultado.Produtos.Length > 0 && resultado.MotivoSemResultado is not null))
      throw new RespostaIaInvalidaException("PRODUTOS_INCONSISTENTES");

    foreach (var produto in resultado.Produtos)
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
  }

  private static bool UrlPublica(string? valor) =>
    Uri.TryCreate(valor, UriKind.Absolute, out var uri)
    && uri.Scheme is "http" or "https"
    && uri.HostNameType == UriHostNameType.Dns
    && uri.Host.Contains('.')
    && !uri.IsLoopback
    && string.IsNullOrEmpty(uri.UserInfo);
}

