namespace Verum.Modules.Assinaturas.Contratos;

public interface IWebhookPagamentoService
{
  Task ReceberAsync(string gateway, string recursoId, string tipo, string assinatura, string requestId, CancellationToken cancellationToken);
}
