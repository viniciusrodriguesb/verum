using Verum.Modules.Assinaturas.Aplicacao.Gateways.DTOs;

namespace Verum.Modules.Assinaturas.Aplicacao.Gateways;

// Cada operação permanece no gateway que originou a assinatura. Trocar automaticamente uma cobrança incerta pode duplicá-la.
internal interface IGatewayAssinaturas
{
  string Nome { get; }

  bool ValidarWebhook(string recursoId, string assinatura, string requestId);

  Task<AssinaturaGateway> CriarAsync(Guid referencia, string email, string plano, decimal valor, CancellationToken cancellationToken);

  Task<AssinaturaGateway?> LocalizarAsync(Guid referencia, CancellationToken cancellationToken);

  Task<AssinaturaGateway> ObterAsync(string id, CancellationToken cancellationToken);

  Task CancelarAsync(string id, CancellationToken cancellationToken);

  Task<IReadOnlyList<CobrancaGateway>> ConsultarCobrancasAsync(string assinaturaId, CancellationToken cancellationToken);

  Task<string?> ResolverAssinaturaAsync(string tipo, string recursoId, CancellationToken cancellationToken);
}
