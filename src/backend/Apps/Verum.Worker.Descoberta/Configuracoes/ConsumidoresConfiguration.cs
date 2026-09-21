using MassTransit;
using Verum.Worker.Descoberta.Consumidores;

namespace Verum.Worker.Descoberta.Configuracoes;

public static class ConsumidoresConfiguration
{
  public static void AddConsumidoresDescoberta(this IBusRegistrationConfigurator bus) =>
    bus.AddConsumer<BuscaSolicitadaConsumer>();
}

