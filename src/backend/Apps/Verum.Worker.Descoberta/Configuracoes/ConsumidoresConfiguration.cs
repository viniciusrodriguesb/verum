using MassTransit;
using Verum.Modules.Busca.Contratos;
using Verum.Worker.Descoberta.Consumidores;

namespace Verum.Worker.Descoberta.Configuracoes;

public static class ConsumidoresConfiguration
{
  public static void AddConsumidoresDescoberta(this IBusRegistrationConfigurator bus, string? fila = null)
  {
    bus.AddConsumer<BuscaSolicitadaConsumer>()
       .Endpoint(endpoint => endpoint.Name = fila ?? BuscaSolicitada.FilaPadrao);

    bus.AddConsumer<BuscaFalhouConsumer>()
       .Endpoint(endpoint => endpoint.Name = (fila ?? BuscaSolicitada.FilaPadrao) + "-falhas");
  }
}
