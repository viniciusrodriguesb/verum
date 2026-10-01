using MassTransit;
using Verum.Worker.Descoberta.Consumidores;
using Verum.Modules.Busca.Contratos;

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
