using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using Verum.BuildingBlocks.Erros;
using Verum.Modules.Assinaturas.Aplicacao.Configuracoes;
using Verum.Modules.Assinaturas.Aplicacao.Gateways;
using Verum.Modules.Assinaturas.Contratos;
using Verum.Modules.Assinaturas.Dominio;
using Verum.Modules.Assinaturas.Infraestrutura.Persistencia;

namespace Verum.Modules.Assinaturas.Aplicacao;

internal sealed class WebhookPagamentoService(AssinaturasDbContext db, SelecionadorGateway gateways,
  IOptions<AssinaturasOptions> options) : IWebhookPagamentoService
{
  public async Task ReceberAsync(string gateway, string recursoId, string tipo, string assinatura, string requestId, CancellationToken cancellationToken)
  {
    if (!options.Value.Habilitado)
      throw new ErroAplicacaoException("PAGAMENTOS_DESABILITADOS", "Pagamentos indisponíveis.", TipoErro.Indisponivel);

    if (!gateways.Obter(gateway).ValidarWebhook(recursoId, assinatura, requestId))
      throw new UnauthorizedAccessException("Assinatura do webhook inválida.");

    if (tipo is not ("payment" or "subscription_preapproval" or "subscription_authorized_payment")) return;

    var chave = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{tipo}:{recursoId}:{requestId}")));

    db.EventosGateway.Add(new EventoGateway(Enum.Parse<GatewayPagamento>(gateway), chave, tipo,
      JsonSerializer.Serialize(new { recursoId })));

    try
    {
      // Recebimento durável antes do HTTP 200; nenhuma chamada externa nesta etapa.
      await db.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateException erro) when (erro.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
    {
      db.ChangeTracker.Clear();
    }
  }
}
