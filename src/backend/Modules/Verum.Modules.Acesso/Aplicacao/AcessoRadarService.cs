using Microsoft.EntityFrameworkCore;
using Verum.Modules.Acesso.Contratos;
using Verum.Modules.Acesso.Infraestrutura.Persistencia;

namespace Verum.Modules.Acesso.Aplicacao;

internal sealed class AcessoRadarService(AcessoDbContext db) : IAcessoRadarService
{
  public Task<bool> PermitidoAsync(Guid contaId, CancellationToken cancellationToken)
  {
    var agora = DateTimeOffset.UtcNow;

    return db.Concessoes.AsNoTracking().AnyAsync(x => x.ContaId == contaId && x.RevogadaEm == null
      && x.ValidaDe <= agora && (x.ValidaAte == null || x.ValidaAte > agora)
      && x.Pacote.Ativo && x.Pacote.Codigo == "VERUM_PLUS", cancellationToken);
  }
}
