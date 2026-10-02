using Verum.Modules.Busca.Aplicacao.Descoberta;
using Verum.Modules.Ofertas.Contratos.Descoberta.DTOs;
using Xunit;

namespace Verum.Tests.Integracao;

public sealed class RankingOfertasTests
{
  [Theory]
  [InlineData("playstation 5", "Console PlayStation 5 Slim com Controle", "Controle Sony DualSense PlayStation 5")]
  [InlineData("iphone 17", "Smartphone Apple iPhone 17", "Capinha para iPhone 17")]
  [InlineData("ps5", "Console PlayStation 5", "Controle PS5")]
  [InlineData("samsung s25", "Smartphone Samsung Galaxy S25", "Gshield Película para Samsung Galaxy S25")]
  [InlineData("samsung s25", "Samsung Galaxy S25 256GB", "Fone De Ouvido Bluetooth Para Samsung S25")]
  [InlineData("samsung s25", "Samsung Galaxy S25 256GB", "JETech Protetor de Tela para Samsung Galaxy S25")]
  public void ProdutoPrincipalPrecedeAcessorioMesmoComConfiancaInferior(string consulta, string principal, string acessorio)
  {
    var resultado = RankingOfertas.Classificar([Oferta(acessorio, 100m), Oferta(principal, 50m)], 12, consulta);

    Assert.Equal(principal, resultado[0].Nome);
  }

  [Fact]
  public void BuscaPorAcessorioNaoRecebePenalizacaoDeAcessorio()
  {
    Assert.True(NormalizacaoOferta.Relevancia("controle ps5", "Controle Sony DualSense PS5") > 30m);
  }

  [Fact]
  public void TelevisaoCorrespondeATvSemPerderResolucao()
  {
    Assert.True(NormalizacaoOferta.Corresponde("televisão 4k", "Smart TV Samsung 55 4K"));

    Assert.False(NormalizacaoOferta.Corresponde("televisão 4k", "Smart TV Samsung Full HD"));
  }

  [Fact]
  public void SelecionaDozeValidasSemDuplicatasModelosDiferentesOuExpiradas()
  {
    var ofertas = Enumerable.Range(0, 16).Select(i => Oferta("SSD Kingston 1TB " + i, 50m)).ToList();

    ofertas.Add(ofertas[0]);

    ofertas.Add(Oferta("SSD Kingston 2TB", 100m));

    ofertas.Add(Oferta("SSD Kingston 1TB", 100m) with { ValidaAte = DateTimeOffset.UtcNow.AddMinutes(-1) });

    var resultado = RankingOfertas.Classificar(ofertas, 12, "ssd kingston 1tb");

    Assert.Equal(12, resultado.Count);

    Assert.Equal(12, resultado.Select(x => x.Url).Distinct().Count());

    Assert.Equal(Enumerable.Range(1, 12), resultado.Select(x => x.Posicao));
  }

  [Theory]
  [InlineData("amazon", "R$ 1.299,90", "preco")]
  [InlineData("teste", "1,299.90", "precoPix")]
  [InlineData("kabum", "1.299,90", "precoPix")]
  public void NormalizaPrecoTotalSemInventarPix(string fonte, string preco, string campo)
  {
    var item = new ItemConsultaLoja("https://loja.example/produto/1", new Dictionary<string, string?>
    {
      ["nome"] = "SSD Kingston 1TB",

      [campo] = preco
    });

    var oferta = NormalizacaoOferta.DaLoja(item, fonte, "ssd kingston", DateTimeOffset.UtcNow);

    Assert.NotNull(oferta);

    Assert.Equal(1299.90m, oferta.Preco);

    Assert.Equal(campo == "precoPix", oferta.CondicaoPreco.Contains("PIX"));
  }

  private static OfertaConsultada Oferta(string nome, decimal confianca) => new(
    Guid.NewGuid(), Guid.NewGuid(), nome, "https://loja.example/produto/" + Guid.NewGuid(), 500m,
    "Preço anunciado", null, "teste", confianca, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMinutes(15));
}
