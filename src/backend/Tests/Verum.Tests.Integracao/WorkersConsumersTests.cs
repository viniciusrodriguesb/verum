using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Verum.CrossCutting.Mensageria;
using Verum.CrossCutting.Pipelines;
using Verum.Modules.Busca.Contratos;
using Verum.Modules.Radar.Contratos;
using Verum.Worker.Descoberta.Configuracoes;
using Verum.Worker.Descoberta.Consumidores;
using Verum.Worker.Radar.Configuracoes;
using Verum.Worker.Radar.Consumidores;
using Xunit;

namespace Verum.Tests.Integracao;

public sealed class WorkersConsumersTests
{
  [Theory]
  [InlineData(true)]
  [InlineData(false)]
  public Task DescobertaRecebeMensagemSemConfirmarProcessamentoInexistente(bool idValido) =>
    VerificarEsqueleto(new BuscaSolicitada(idValido ? Guid.NewGuid() : Guid.Empty),
      bus => bus.AddConsumidoresDescoberta(), "descoberta", typeof(VerificacaoRadarSolicitadaConsumer),
      idValido ? typeof(ProcessamentoNaoConfiguradoException) : typeof(ArgumentException));

  [Theory]
  [InlineData(true)]
  [InlineData(false)]
  public Task RadarRecebeMensagemSemConfirmarProcessamentoInexistente(bool idValido) =>
    VerificarEsqueleto(new VerificacaoRadarSolicitada(idValido ? Guid.NewGuid() : Guid.Empty),
      bus => bus.AddConsumidoresRadar(), "radar", typeof(BuscaSolicitadaConsumer),
      idValido ? typeof(ProcessamentoNaoConfiguradoException) : typeof(ArgumentException));

  private static async Task VerificarEsqueleto<T>(T mensagem,
    Action<IBusRegistrationConfigurator> registrar, string host, Type consumidorOutroWorker, Type erro)
    where T : class
  {
    var fault = new TaskCompletionSource<Fault<T>>(TaskCreationOptions.RunContinuationsAsynchronously);
    var observer = new TentativasObserver<T>();
    var services = new ServiceCollection().AddLogging();
    services.AddMassTransit(bus =>
    {
      bus.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter($"verum-{host}", false));
      registrar(bus);
      bus.AddHandler<Fault<T>>(context =>
      {
        fault.TrySetResult(context.Message);
        return Task.CompletedTask;
      });
      bus.AddConfigureEndpointsCallback((context, _, endpoint) =>
        endpoint.ConfigureVerumConsumer(context, new RabbitMqOptions()));
      bus.UsingInMemory((context, transport) => transport.ConfigureEndpoints(context));
    });
    Assert.DoesNotContain(services, service => service.ServiceType == consumidorOutroWorker);
    await using var provider = services.BuildServiceProvider(true);
    var bus = provider.GetRequiredService<IBusControl>();
    using var connection = bus.ConnectConsumeObserver(observer);
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
    await bus.StartAsync(timeout.Token);
    try
    {
      await bus.Publish(mensagem, timeout.Token);
      var result = await fault.Task.WaitAsync(timeout.Token);
      Assert.Contains(result.Exceptions, exception => exception.ExceptionType == erro.FullName);
      Assert.Equal(1, observer.Tentativas);
    }
    finally { await bus.StopAsync(CancellationToken.None); }
  }

  [Fact]
  public void FilasTemNomesSeparadosPorWorker()
  {
    Assert.Equal("verum-descoberta-busca-solicitada",
      new KebabCaseEndpointNameFormatter("verum-descoberta", false).Consumer<BuscaSolicitadaConsumer>());
    Assert.Equal("verum-radar-verificacao-radar-solicitada",
      new KebabCaseEndpointNameFormatter("verum-radar", false).Consumer<VerificacaoRadarSolicitadaConsumer>());
  }

  [Fact]
  public async Task PipelineConsomeEmParaleloRespeitandoLimiteSemSemaforo()
  {
    const int total = 6;
    var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    var finished = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    int active = 0, peak = 0, completed = 0;
    var services = new ServiceCollection().AddLogging();
    services.AddMassTransit(bus =>
    {
      bus.AddHandler<MensagemParalelismo>(async context =>
      {
        var current = Interlocked.Increment(ref active);
        int previous;
        do { previous = Volatile.Read(ref peak); }
        while (current > previous && Interlocked.CompareExchange(ref peak, current, previous) != previous);
        if (current >= 2) entered.TrySetResult(true);
        try
        {
          await release.Task.WaitAsync(context.CancellationToken);
          await Task.Delay(20, context.CancellationToken);
        }
        finally { Interlocked.Decrement(ref active); }
        if (Interlocked.Increment(ref completed) == total) finished.TrySetResult(true);
      });
      bus.AddConfigureEndpointsCallback((context, _, endpoint) =>
        endpoint.ConfigureVerumConsumer(context, new RabbitMqOptions { ConcurrentMessageLimit = 2 }));
      bus.UsingInMemory((context, transport) => transport.ConfigureEndpoints(context));
    });
    await using var provider = services.BuildServiceProvider(true);
    var bus = provider.GetRequiredService<IBusControl>();
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
    await bus.StartAsync(timeout.Token);
    try
    {
      await Task.WhenAll(Enumerable.Range(0, total)
        .Select(_ => bus.Publish(new MensagemParalelismo(Guid.NewGuid()), timeout.Token)));
      await entered.Task.WaitAsync(timeout.Token);
      Assert.Equal(2, Volatile.Read(ref active));
      release.TrySetResult(true);
      await finished.Task.WaitAsync(timeout.Token);
      Assert.Equal(2, peak);
      Assert.Equal(total, completed);
    }
    finally
    {
      release.TrySetResult(true);
      await bus.StopAsync(CancellationToken.None);
    }
  }

  private sealed class TentativasObserver<TAlvo> : IConsumeObserver where TAlvo : class
  {
    public int Tentativas;
    public Task PreConsume<T>(ConsumeContext<T> context) where T : class
    {
      if (typeof(T) == typeof(TAlvo)) Interlocked.Increment(ref Tentativas);
      return Task.CompletedTask;
    }
    public Task PostConsume<T>(ConsumeContext<T> context) where T : class => Task.CompletedTask;
    public Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class => Task.CompletedTask;
  }
}

public sealed record MensagemParalelismo(Guid Id);

