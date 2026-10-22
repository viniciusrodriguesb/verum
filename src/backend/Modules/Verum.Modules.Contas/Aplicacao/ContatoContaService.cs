using Microsoft.EntityFrameworkCore;
using Verum.Modules.Contas.Contratos;
using Verum.Modules.Contas.Dominio;
using Verum.Modules.Contas.Infraestrutura.Persistencia;

namespace Verum.Modules.Contas.Aplicacao;

internal sealed class ContatoContaService(ContasDbContext db) : IContatoContaService
{
  public Task<string?> EmailAtivoAsync(Guid contaId, CancellationToken cancellationToken) =>
    db.Contas.AsNoTracking().Where(x => x.Id == contaId && x.Status == StatusConta.Ativa)
      .Select(x => (string?)x.Email).SingleOrDefaultAsync(cancellationToken);
}
