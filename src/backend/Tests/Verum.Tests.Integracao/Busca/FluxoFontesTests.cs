using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Verum.BuildingBlocks.InteligenciaArtificial.DTOs;
using Verum.Modules.Busca.Aplicacao.Descoberta;
using Verum.Modules.Busca.Aplicacao.Descoberta.Configuracoes;
using Verum.Modules.Busca.Contratos.InteligenciaArtificial;
using Verum.Modules.Busca.Contratos.InteligenciaArtificial.DTOs;
using Verum.Modules.Ofertas.Contratos.Descoberta;
using Verum.Modules.Ofertas.Contratos.Descoberta.DTOs;
using Xunit;

namespace Verum.Tests.Integracao;

public sealed class FluxoFontesTests
{
  [Fact]
  public async Task BloqueioDeLojaEhDiagnosticadoEPreservaOutrasFontes()
  {
    var resultado = await Service(new LojaFake(), new IaFake(), minimo: 1, lojas: ["bloqueada", "kabum"])
      .ConsultarAsync("ssd kingston", 0, CancellationToken.None);

    Assert.Single(resultado.Ofertas);

    Assert.Contains(resultado.Fontes, x => x.Fonte == "bloqueada" && x.Codigo == "FONTE_BLOQUEADA");
  }

  [Fact]
  public async Task IaSemConfiguracaoNaoEhChamadaEPreservaLoja()
  {
    var ia = new IaFake { Disponivel = false };

    var resultado = await Service(new LojaFake(), ia).ConsultarAsync("ssd kingston", 0, CancellationToken.None);

    Assert.Single(resultado.Ofertas);

    Assert.Equal(0, ia.Chamadas);

    Assert.Contains(resultado.Fontes, x => x.Codigo == "IA_NAO_CONFIGURADA");
  }

  [Theory]
  [InlineData(false)]
  [InlineData(true)]
  public async Task FalhaOuTimeoutDaIaPreservaOfertas(bool demora)
  {
    var ia = new IaFake { Demora = demora, Falha = !demora };

    var resultado = await Service(new LojaFake(), ia).ConsultarAsync("ssd kingston", 0, CancellationToken.None);

    Assert.Single(resultado.Ofertas);

    Assert.Contains(resultado.Fontes, x => x.Fonte == "ia" && x.Status == "Falhou"
      && x.Codigo == (demora ? "TIMEOUT" : "IA_INDISPONIVEL"));
  }

  [Fact]
  public async Task FontesIndependentesNaoSeInterrompemEResultadoSuficienteEvitaIa()
  {
    var ia = new IaFake { Falha = true };

    var resultado = await Service(new LojaFake(), ia, minimo: 1, lojas: ["falha", "kabum"])
      .ConsultarAsync("ssd kingston", 0, CancellationToken.None);

    Assert.Single(resultado.Ofertas);

    Assert.Contains(resultado.Fontes, x => x.Fonte == "falha" && x.Status == "Falhou");

    Assert.Equal(0, ia.Chamadas);
  }

  [Fact]
  public async Task IaRecuperaFalhaDaLojaEDescartaPrecoDeParcelaOuModeloDiferente()
  {
    var correto = Candidato();

    var ia = new IaFake
    {
      Resposta = Resposta(correto, correto with { Nome = "SSD OutraMarca", Url = "https://loja.example/outro" },
        correto with { PrecoPix = null, Preco = null, QuantidadeParcelas = 10, ValorParcela = 49.99m })
    };

    var resultado = await Service(new LojaFake { Falha = true }, ia)
      .ConsultarAsync("ssd kingston", 0, CancellationToken.None);

    Assert.Single(resultado.Ofertas);

    Assert.True(resultado.Ofertas[0].OrigemIa);

    Assert.Contains(resultado.Fontes, x => x.Fonte == "ia" && x.Aceitos == 1 && x.Encontrados == 3);
  }

  [Fact]
  public async Task IaComplementaLojaSemDuplicarUrlJaEncontrada()
  {
    var ia = new IaFake
    {
      Resposta = Resposta(Candidato() with { Url = Item().Url }, Candidato() with { Url = "https://loja.example/ssd-kingston" })
    };

    var resultado = await Service(new LojaFake(), ia).ConsultarAsync("ssd kingston", 0, CancellationToken.None);

    Assert.Equal(2, resultado.Ofertas.Count);

    Assert.False(resultado.Ofertas[0].OrigemIa);
  }

  [Fact]
  public async Task PrazoTotalPreservaFonteRapidaEnquantoCancelaFonteLenta()
  {
    using var prazo = new CancellationTokenSource(TimeSpan.FromMilliseconds(150));

    var resultado = await Service(new LojaFake(), new IaFake(), lojas: ["kabum", "lenta"])
      .ConsultarAsync("ssd kingston", 0, prazo.Token);

    Assert.Single(resultado.Ofertas);

    Assert.Contains(resultado.Fontes, x => x.Fonte == "lenta" && x.Codigo == "TIMEOUT");
  }

  [Fact]
  public async Task CancelamentoDoHostNaoViraFalhaDeFonte()
  {
    using var parada = new CancellationTokenSource();

    parada.Cancel();

    await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
      Service(new LojaFake(), new IaFake()).ConsultarAsync("ssd kingston", 0, parada.Token, parada.Token));
  }

  [Theory]
  [InlineData("12x de 99,90")]
  [InlineData("0,00")]
  [InlineData("1.2,34")]
  [InlineData("1,234")]
  public void PrecoAmbiguoOuInvalidoNaoViraOferta(string preco)
  {
    Assert.Null(NormalizacaoOferta.DaLoja(Item(preco), "kabum", "ssd kingston", DateTimeOffset.UtcNow));
  }

  [Fact]
  public void ModeloDiferenteNaoViraCorrespondencia()
  {
    Assert.False(NormalizacaoOferta.Corresponde("iphone 16 256gb", "iPhone 16 128GB"));

    Assert.True(NormalizacaoOferta.Corresponde("iphone 16 256gb", "iPhone 16 256 GB"));
  }

  internal static ProdutoCandidatoIa Candidato() => new()
  {
    Nome = "SSD Kingston NV3 1TB",
    Marca = "Kingston",
    Modelo = "NV3",
    Variante = "1TB",
    Gtin = null,
    Loja = "Loja de teste",
    Vendedor = null,
    Url = "https://loja.example/produto/123",
    ImagemUrl = null,
    Preco = 550m,
    PrecoPix = 499.90m,
    PrecoAnterior = null,
    QuantidadeParcelas = null,
    ValorParcela = null,
    CondicaoPreco = "No PIX",
    Moeda = "BRL",
    Disponibilidade = "disponivel",
    CondicaoProduto = "novo",
    Correspondencia = "exata",
    Evidencia = "Preço observado na fonte pública de teste."
  };

  internal static DescobertaProdutosIa Resposta(params ProdutoCandidatoIa[] correcto) =>
    new(new ResultadoProdutosIa { ConsultaInterpretada = "ssd kingston", MotivoSemResultado = null, Produtos = correcto },
      new RespostaIa(JsonSerializer.SerializeToElement(new { }), "Teste", "simulado", "resposta-teste",
        DateTimeOffset.UtcNow, null, null, correcto.Select(x => new FonteIa(x.Url, x.Nome)).ToArray()));

  private static ConsultaFontesService Service(LojaFake loja, IaFake ia, int minimo = 3, string[]? lojas = null) =>
    new(loja, ia, Options.Create(new BuscaOptions { TimeoutIaSegundos = 1, MinimoOfertas = minimo, Lojas = lojas ?? ["kabum"] }),
      NullLogger<ConsultaFontesService>.Instance);

  internal static ItemConsultaLoja Item(string preco = "499,90") =>
    new("https://www.kabum.com.br/produto/123/ssd-kingston", new Dictionary<string, string?>
    {
      ["nome"] = "SSD Kingston NV3 1TB",
      ["precoPix"] = preco
    });

  internal sealed class LojaFake : IConsultaLoja
  {
    public int Chamadas;

    public bool Falha { get; set; }

    public bool Vazia { get; set; }

    public async Task<ResultadoConsultaLoja> ConsultarAsync(string loja, string consulta, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();

      Interlocked.Increment(ref Chamadas);

      if (Falha || loja == "falha") throw new HttpRequestException("Simulação");

      if (loja == "bloqueada") throw new HttpRequestException("Bloqueio simulado", null, System.Net.HttpStatusCode.Forbidden);

      if (loja == "lenta") await Task.Delay(Timeout.Infinite, cancellationToken);

      return new ResultadoConsultaLoja(loja, DateTimeOffset.UtcNow, Vazia ? [] : [Item()], [], false);
    }
  }

  internal sealed class IaFake : IDescobertaProdutosIaService
  {
    public bool Disponivel { get; set; } = true;

    public bool Demora { get; set; }

    public bool Falha { get; set; }

    public int Chamadas;

    public DescobertaProdutosIa? Resposta { get; set; }

    public async Task<DescobertaProdutosIa> ConsultarAsync(string consulta, CancellationToken cancellationToken = default)
    {
      Chamadas++;

      if (Demora) await Task.Delay(Timeout.Infinite, cancellationToken);

      if (Falha) throw new HttpRequestException("Chave inválida simulada");

      if (Resposta is not null) return Resposta;

      throw new InvalidOperationException("A simulação não deveria consultar IA neste cenário.");
    }
  }
}
