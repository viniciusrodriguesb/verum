using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Verum.Modules.Busca;
using Verum.Modules.Busca.Aplicacao.Descoberta.Configuracoes;
using Xunit;

namespace Verum.Tests.Integracao;

public sealed class ConfiguracaoLojasTests
{
  [Fact]
  public void ListaConfiguradaSubstituiPadroesParaPermitirDesativarUmaFonte()
  {
    var configuracao = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
      ["Busca:Lojas:0"] = "kalunga"
    }).Build();

    var services = new ServiceCollection();

    services.AddBuscaPersistencia(configuracao);

    using var provider = services.BuildServiceProvider();

    Assert.Equal(["kalunga"], provider.GetRequiredService<IOptions<BuscaOptions>>().Value.Lojas);
  }
}
