using Verum.Modules.Busca.Aplicacao.Descoberta;
using Verum.Modules.Ofertas.Contratos.Descoberta.DTOs;
using Xunit;

namespace Verum.Tests.Integracao;

public sealed class RankingEstatisticoTests
{
  [Fact]
  public void OfertaUnicaTemPrecoProvisorioENaoNotaMaxima()
  {
    var resultado = RankingOfertas.Classificar([Oferta(100m)], 12, "ssd kingston").Single();

    Assert.Equal(50m, resultado.PontuacaoPreco);

    Assert.True(resultado.Fundamentos!.PrecoProvisorio);

    Assert.Equal("Baixa", resultado.Fundamentos.ConfiancaCalculo);

    Assert.Equal(decimal.Round(resultado.PontuacaoPreco * .45m + resultado.PontuacaoConfianca * .35m
      + resultado.PontuacaoAtualizacao * .20m, 2), resultado.Pontuacao);
  }

  [Fact]
  public void RepetirColetasDaMesmaLojaNaoFabricaConfiancaEstatistica()
  {
    var loja = Guid.NewGuid();

    var oferta = Oferta(100m) with
    {
      Historico = Enumerable.Range(0, 100).Select(_ => new ObservacaoPrecoRanking(loja, DateTimeOffset.UtcNow.AddMinutes(-1), 100m, false)).ToArray()
    };

    var resultado = RankingOfertas.Classificar([oferta], 12, "ssd kingston").Single();

    Assert.Equal(1, resultado.Fundamentos!.Observacoes);

    Assert.Equal(1, resultado.Fundamentos.LojasHistorico);

    Assert.True(resultado.Fundamentos.PrecoProvisorio);
  }

  [Fact]
  public void AtualizacaoUsaMeiaVidaDeUmaHora()
  {
    var oferta = Oferta(100m) with { ObservadaEm = DateTimeOffset.UtcNow.AddHours(-1) };

    var resultado = RankingOfertas.Classificar([oferta], 12, "ssd kingston").Single();

    Assert.InRange(resultado.PontuacaoAtualizacao, 49.99m, 50m);
  }

  [Fact]
  public void MercadoComparavelMantemEmpatesPequenosEExcluiOutraVariante()
  {
    var variante = Guid.NewGuid();

    var ofertas = new[] { Oferta(100m), Oferta(100.01m), Oferta(100.02m) }
      .Select(x => x with { VarianteId = variante, LojaId = Guid.NewGuid() }).ToArray();

    var resultado = RankingOfertas.Classificar(ofertas.Append(Oferta(1m)), 12, "ssd kingston");

    Assert.All(resultado.Where(x => x.VarianteId == variante), x => Assert.Equal(50m, x.PontuacaoPreco));

    Assert.All(resultado.Where(x => x.VarianteId == variante), x => Assert.Equal(3, x.Fundamentos!.LojasMercado));
  }

  private static OfertaConsultada Oferta(decimal preco) => new(Guid.NewGuid(), Guid.NewGuid(), "SSD Kingston", "https://loja.example/" + Guid.NewGuid(),
    preco, "Preço anunciado", null, "teste", 70m, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(2));
}
