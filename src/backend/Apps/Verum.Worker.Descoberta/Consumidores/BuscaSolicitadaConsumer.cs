using MassTransit;
using Microsoft.Extensions.Logging;
using Verum.CrossCutting.Mensageria;
using Verum.Modules.Busca.Contratos;

namespace Verum.Worker.Descoberta.Consumidores;

public sealed class BuscaSolicitadaConsumer(ILogger<BuscaSolicitadaConsumer> logger)
  : ConsumidorMensagem<BuscaSolicitada>(logger)
{
  protected override Task ProcessarAsync(ConsumeContext<BuscaSolicitada> context)
  {
    context.CancellationToken.ThrowIfCancellationRequested();
    if (context.Message.BuscaId == Guid.Empty)
      throw new ArgumentException("BuscaId deve ser informado.");

    #region Processamento da descoberta
    // TODO: injetar o serviço/fachada de Busca no construtor e substituir o throw:
    // return processamento.ProcessarAsync(context.Message.BuscaId, context.CancellationToken);
    // Aguardar o processamento completo; não usar Task.Run ou fire-and-forget.
    // O serviço será responsável pela idempotência e pelo fallback entre fontes.
    throw new ProcessamentoNaoConfiguradoException(nameof(BuscaSolicitadaConsumer));
    #endregion
  }
}

