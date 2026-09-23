using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Verum.CrossCutting.Mensageria;
using Verum.CrossCutting.Pipelines;
using Xunit;

namespace Verum.Tests.Integracao;

public record Solicitada(Guid Id);
public record Concluida(Guid Id);
public record Invalida(Guid Id);

public sealed class Contador
{
  public int Tentativas;

  public int Concluidas;

  public int Invalidas;

  public TaskCompletionSource<bool> Conclusao { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

  public TaskCompletionSource<bool> Falha { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
}

public sealed class RecuperavelConsumer(Contador contador, ILogger<RecuperavelConsumer> logger)
  : ConsumidorMensagem<Solicitada>(logger)
{
  protected override async Task ProcessarAsync(ConsumeContext<Solicitada> context)
  {
    await context.Publish(new Concluida(context.Message.Id));

    if (Interlocked.Increment(ref contador.Tentativas) < 3) throw new HttpRequestException("Falha transitória");
  }
}

public sealed class ConcluidaConsumer(Contador contador) : IConsumer<Concluida>
{
  public Task Consume(ConsumeContext<Concluida> context)
  {
    Interlocked.Increment(ref contador.Concluidas);

    contador.Conclusao.TrySetResult(true);

    return Task.CompletedTask;
  }
}

public sealed class InvalidaConsumer(Contador contador) : IConsumer<Invalida>
{
  public Task Consume(ConsumeContext<Invalida> context)
  {
    Interlocked.Increment(ref contador.Invalidas);

    throw new ArgumentException("Mensagem inválida");
  }
}

public sealed class FalhaConsumer(Contador contador) : IConsumer<Fault<Invalida>>
{
  public Task Consume(ConsumeContext<Fault<Invalida>> context)
  {
    contador.Falha.TrySetResult(true);

    return Task.CompletedTask;
  }
}

public sealed class MensageriaTests
{
  [Fact]
  public async Task RetryOutboxEFaultPreservamSemanticaDeEntrega()
  {
    var services = new ServiceCollection().AddLogging();

    services.AddSingleton<Contador>();

    services.Configure<RabbitMqOptions>(_ => { });

    services.AddVerumPublishers();

    services.AddMassTransit(bus =>
    {
      bus.AddConsumer<RecuperavelConsumer>();

      bus.AddConsumer<ConcluidaConsumer>();

      bus.AddConsumer<InvalidaConsumer>();

      bus.AddConsumer<FalhaConsumer>();

      bus.AddConfigureEndpointsCallback((context, _, endpoint) =>
        endpoint.ConfigureVerumConsumer(context, new RabbitMqOptions()));

      bus.UsingInMemory((context, transport) => transport.ConfigureEndpoints(context));
    });

    await using var provider = services.BuildServiceProvider(true);

    var bus = provider.GetRequiredService<IBusControl>();

    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));

    await bus.StartAsync(timeout.Token);

    try
    {
      using var scope = provider.CreateScope();

      await scope.ServiceProvider.GetRequiredService<PublicadorMensagem<Solicitada>>()
        .PublicarAsync(new Solicitada(Guid.NewGuid()), timeout.Token);

      var contador = provider.GetRequiredService<Contador>();

      await contador.Conclusao.Task.WaitAsync(timeout.Token);

      Assert.Equal(3, contador.Tentativas);

      Assert.Equal(1, contador.Concluidas);

      await scope.ServiceProvider.GetRequiredService<PublicadorMensagem<Invalida>>()
        .PublicarAsync(new Invalida(Guid.NewGuid()), timeout.Token);

      await contador.Falha.Task.WaitAsync(timeout.Token);

      Assert.Equal(1, contador.Invalidas);
    }
    finally {
      await bus.StopAsync(CancellationToken.None);
    }
  }
}

