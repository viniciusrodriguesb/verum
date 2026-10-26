using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Verum.Modules.Assinaturas;
using Verum.Modules.Assinaturas.Aplicacao.Configuracoes;
using Verum.Modules.Notificacoes;
using Verum.Modules.Notificacoes.Aplicacao.Configuracoes;
using Xunit;

namespace Verum.Tests.Integracao;

public sealed class LimitesProcessamentoTests
{
  [Fact]
  public void RejeitaLoteQueImpediriaOProcessamentoDePagamentos()
  {
    var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
      ["Assinaturas:EventosPorCiclo"] = "0"
    }).Build();

    var services = new ServiceCollection();

    services.AddAssinaturasPersistencia(config);

    using var provider = services.BuildServiceProvider();

    Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<AssinaturasOptions>>().Value);
  }

  [Fact]
  public void RejeitaLoteQueImpediriaOEnvioDeAvisos()
  {
    var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
      ["Notificacoes:Email:LoteEnvio"] = "0"
    }).Build();

    var services = new ServiceCollection();

    services.AddNotificacoesPersistencia(config);

    using var provider = services.BuildServiceProvider();

    Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<EmailOptions>>().Value);
  }
}
