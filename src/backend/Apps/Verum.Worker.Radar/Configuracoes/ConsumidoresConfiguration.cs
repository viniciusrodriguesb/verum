using MassTransit;
using Verum.Worker.Radar.Consumidores;

namespace Verum.Worker.Radar.Configuracoes;

public static class ConsumidoresConfiguration
{
  public static void AddConsumidoresRadar(this IBusRegistrationConfigurator bus) =>
    bus.AddConsumer<VerificacaoRadarSolicitadaConsumer>();
}

