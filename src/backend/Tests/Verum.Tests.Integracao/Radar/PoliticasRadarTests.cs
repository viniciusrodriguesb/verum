using Verum.Modules.Busca.Aplicacao.Descoberta;
using Verum.Modules.Notificacoes.Aplicacao;
using Verum.Modules.Notificacoes.Contratos.DTOs;
using Verum.Modules.Radar.Dominio;
using Xunit;

namespace Verum.Tests.Integracao;

public sealed class PoliticasRadarTests
{
  [Theory]
  [InlineData("iphone 15 128gb", "Pen Drive USB compatível com iPhone 15 128GB")]
  [InlineData("iphone 15 128gb", "Apple iPhone 15 Pro 128GB")]
  [InlineData("iphone 15 128gb", "Apple iPhone 15 Plus 128GB")]
  [InlineData("iphone 15 128gb", "Apple iPhone 15 128GB Seminovo")]
  public void AcessoriosEdicoesECondicoesNaoSolicitadasNaoSaoElegiveis(string consulta, string titulo) =>
    Assert.True(NormalizacaoOferta.Relevancia(consulta, titulo) <= 30);

  [Fact]
  public void CooldownEHistereseExigemSaidaDaFaixaAntesDeNovoAlerta()
  {
    var item = new Monitoramento(Guid.NewGuid(), Guid.NewGuid(), "iPhone 15 128GB", 1000, 900, Guid.NewGuid());

    var agora = DateTimeOffset.UtcNow;

    Assert.True(item.Avaliar(900, Guid.NewGuid(), 1, agora, 360, 1440, 2));

    Assert.False(item.Avaliar(890, Guid.NewGuid(), 2, agora.AddDays(2), 360, 1440, 2));

    Assert.False(item.Avaliar(910, Guid.NewGuid(), 3, agora.AddDays(2), 360, 1440, 2));

    Assert.False(item.Avaliar(950, Guid.NewGuid(), 4, agora.AddDays(2), 360, 1440, 2));

    Assert.True(item.Avaliar(880, Guid.NewGuid(), 5, agora.AddDays(2), 360, 1440, 2));
  }

  [Fact]
  public void PausarOuEncerrarNaoPermiteAlterarMetaPeloMesmoComando()
  {
    var item = new Monitoramento(Guid.NewGuid(), Guid.NewGuid(), "Livro", 100, 90, Guid.NewGuid());

    item.Alterar(1, "pausar");

    Assert.Equal(90, item.PrecoAlvo);

    item.Alterar(1, "encerrar");

    Assert.Equal(90, item.PrecoAlvo);
  }

  [Fact]
  public void FonteIndisponivelNaoSignificaFaltaDeEstoque()
  {
    var item = new Monitoramento(Guid.NewGuid(), Guid.NewGuid(), "Livro", 100, 90, Guid.NewGuid());

    Assert.False(item.Avaliar(null, null, null, DateTimeOffset.UtcNow, 360, 1440, 2));

    Assert.Equal("VerificacaoIndisponivel", item.EstadoVerificacao);
  }

  [Theory]
  [InlineData(2, true)]
  [InlineData(12, false)]
  [InlineData(23, true)]
  public void HorarioSilenciosoAtravessaMeiaNoite(int hora, bool esperado) =>
    Assert.Equal(esperado, NotificacoesRadarService.EmSilencio(new PreferenciasAviso(true, 22, 8, "UTC"),
      new DateTimeOffset(2026, 10, 20, hora, 0, 0, TimeSpan.Zero)));
}
