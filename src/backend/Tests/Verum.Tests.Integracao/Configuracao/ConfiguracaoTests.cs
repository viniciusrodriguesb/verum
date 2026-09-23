using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Verum.CrossCutting;
using Verum.CrossCutting.Mensageria;
using Verum.CrossCutting.Pipelines;
using Verum.Tests.Integracao.Aplicacao;
using Xunit;

namespace Verum.Tests.Integracao;

public sealed class ConfiguracaoTests
{
  [Fact]
  public void ServicesSaoScopedECompartilhamInstanciaComContrato()
  {
    var services = new ServiceCollection();

    services.AddVerumServices(typeof(ExemploService).Assembly);

    services.AddVerumServices(typeof(ExemploService).Assembly);

    using var provider = services.BuildServiceProvider(new ServiceProviderOptions
      { ValidateScopes = true, ValidateOnBuild = true });

    using var first = provider.CreateScope();

    using var second = provider.CreateScope();

    var concrete = first.ServiceProvider.GetRequiredService<ExemploService>();

    Assert.Same(concrete, first.ServiceProvider.GetRequiredService<IExemploService>());

    Assert.NotSame(concrete, second.ServiceProvider.GetRequiredService<ExemploService>());

    Assert.NotNull(first.ServiceProvider.GetRequiredService<SemInterfaceService>());

    Assert.Null(first.ServiceProvider.GetService<Dominio.NaoRegistrarService>());

    Assert.DoesNotContain(services, d => d.ServiceType == typeof(GenericoService<>));

    Assert.Single(services, d => d.ServiceType == typeof(IExemploService));
  }

  [Fact]
  public void ServicesRejeitamContratoPreviamenteRegistrado()
  {
    var services = new ServiceCollection().AddScoped<IExemploService, ExemploService>();

    Assert.Throws<InvalidOperationException>(() => services.AddVerumServices(typeof(ExemploService).Assembly));
  }

  [Theory]
  [InlineData("descoberta")]
  [InlineData("radar")]
  public async Task WorkersIniciamEEncerramSemInfraestruturaHabilitada(string worker)
  {
    var builder = Host.CreateApplicationBuilder();

    if (worker == "radar") builder.Services.AddVerumWorkerRadar(builder.Configuration);
    else builder.Services.AddVerumWorkerDescoberta(builder.Configuration);

    using var host = builder.Build();

    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

    await host.StartAsync(timeout.Token);

    Assert.Null(host.Services.GetService<MassTransit.IBus>());

    Assert.NotNull(host.Services.GetRequiredService<IHttpClientFactory>().CreateClient("Verum"));

    await host.StopAsync(timeout.Token);
  }

  [Fact]
  public void RabbitMqRejeitaCredenciaisAusentes()
  {
    var services = new ServiceCollection().AddLogging();

    services.AddVerumMensageria(Config(new() { ["RabbitMQ:Enabled"] = "true" }), "test");

    using var provider = services.BuildServiceProvider();

    Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<RabbitMqOptions>>().Value);
  }

  [Fact]
  public async Task RabbitMqRegistraBusEPublicadorSemConectar()
  {
    var services = new ServiceCollection().AddLogging();

    services.AddSingleton<Contador>();

    services.AddVerumMensageria(Config(new()
    {
      ["RabbitMQ:Enabled"] = "true", ["RabbitMQ:Username"] = "test", ["RabbitMQ:Password"] = "test"
    }), "test", bus => bus.AddConsumer<RecuperavelConsumer>());

    await using var provider = services.BuildServiceProvider();

    await using var scope = provider.CreateAsyncScope();

    Assert.NotNull(scope.ServiceProvider.GetRequiredService<PublicadorMensagem<Solicitada>>());

    Assert.NotNull(provider.GetRequiredService<MassTransit.IBus>());
  }

  [Fact]
  public void RedisHabilitadoExigeConexao()
  {
    Assert.Throws<InvalidOperationException>(() => new ServiceCollection()
      .AddVerumCache(Config(new() { ["Redis:Enabled"] = "true" })));
  }

  [Fact]
  public void JwtSemIssuerFalhaNaValidacao()
  {
    var services = new ServiceCollection().AddLogging();

    services.AddVerumAutenticacao(Config(new() { ["Authentication:Audience"] = "verum-api" }));

    using var provider = services.BuildServiceProvider();

    Assert.Throws<OptionsValidationException>(() =>
      provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(JwtBearerDefaults.AuthenticationScheme));
  }

  internal static IConfiguration Config(Dictionary<string, string?> values) =>
    new ConfigurationBuilder().AddInMemoryCollection(values).Build();
}
