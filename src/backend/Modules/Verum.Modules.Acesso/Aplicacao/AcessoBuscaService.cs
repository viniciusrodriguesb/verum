using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Verum.BuildingBlocks.Erros;
using Verum.BuildingBlocks.Identidade;
using Verum.Modules.Acesso.Aplicacao.Configuracoes;
using Verum.Modules.Acesso.Contratos;
using Verum.Modules.Acesso.Contratos.DTOs;
using Verum.Modules.Acesso.Dominio;
using Verum.Modules.Acesso.Infraestrutura.Persistencia;

namespace Verum.Modules.Acesso.Aplicacao;

internal sealed class AcessoBuscaService(AcessoDbContext db, IOptions<AcessoOptions> options) : IAcessoBuscaService
{
  private const string RecursoBusca = "BUSCA_EXECUTAR";

  public async Task PrepararAsync(AtorBusca ator, CancellationToken cancellationToken)
  {
    await using var transacao = await db.Database.BeginTransactionAsync(cancellationToken);

    await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(710005)", cancellationToken);

    var recurso = await db.Recursos.SingleOrDefaultAsync(x => x.Codigo == RecursoBusca, cancellationToken);

    if (recurso is null)
    {
      recurso = new Recurso(RecursoBusca, "Executar busca", "Solicitar busca de produtos", TipoLimite.Quantidade);

      db.Recursos.Add(recurso);
    }

    var limites = new Dictionary<string, int>
    {
      ["VISITANTE"] = options.Value.LimiteVisitanteDiario,

      ["FREE"] = options.Value.LimiteFreeDiario,

      ["BUSCA_PLUS"] = options.Value.LimitePagoDiario,

      ["VERUM_PLUS"] = options.Value.LimitePagoDiario
    };

    foreach (var (codigo, limite) in limites)
    {
      var pacote = await db.Pacotes.SingleOrDefaultAsync(x => x.Codigo == codigo, cancellationToken);

      if (pacote is null)
      {
        pacote = new PacoteAcesso(codigo, codigo);

        db.Pacotes.Add(pacote);
      }

      var regra = await db.RecursosPacotes.SingleOrDefaultAsync(x => x.PacoteAcessoId == pacote.Id && x.RecursoId == recurso.Id, cancellationToken);

      if (regra is null)
        db.RecursosPacotes.Add(new PacoteRecurso(pacote.Id, recurso.Id, true, limite, PeriodicidadeUso.Dia));
      else
        regra.Configurar(regra.Habilitado, limite, PeriodicidadeUso.Dia);

      if (codigo == "FREE" && ator.ContaId.HasValue
        && !await db.Concessoes.AnyAsync(x => x.ContaId == ator.ContaId && x.PacoteAcessoId == pacote.Id, cancellationToken))
        db.Concessoes.Add(new ConcessaoPacote(ator.ContaId.Value, pacote.Id, OrigemConcessao.Cadastro, DateTimeOffset.UtcNow));
    }

    await db.SaveChangesAsync(cancellationToken);

    await transacao.CommitAsync(cancellationToken);
  }

  public async Task<AcessoBusca> ObterAsync(AtorBusca ator, CancellationToken cancellationToken)
  {
    var agora = DateTimeOffset.UtcNow;

    var politicas = db.RecursosPacotes.AsNoTracking().Where(x => x.Habilitado && x.Pacote.Ativo
      && x.Recurso.Ativo && x.Recurso.Codigo == RecursoBusca && x.Periodicidade == PeriodicidadeUso.Dia);

    DateTimeOffset? validaAte = null;

    PacoteRecurso? politica;

    if (ator.VisitanteId.HasValue)
      politica = await politicas.Include(x => x.Pacote).SingleOrDefaultAsync(x => x.Pacote.Codigo == "VISITANTE", cancellationToken);
    else
    {
      var concedidas = await db.Concessoes.AsNoTracking().Where(x => x.ContaId == ator.ContaId
        && x.RevogadaEm == null && x.ValidaDe <= agora && (x.ValidaAte == null || x.ValidaAte > agora))
        .Join(politicas.Where(x => x.Pacote.Codigo != "VISITANTE"), x => x.PacoteAcessoId, x => x.PacoteAcessoId,
          (concessao, regra) => new { concessao, regra })
        .OrderByDescending(x => x.regra.LimiteQuantidade).ThenBy(x => x.regra.Pacote.Codigo)
        .Select(x => new { x.regra.PacoteAcessoId, x.concessao.ValidaAte }).FirstOrDefaultAsync(cancellationToken);

      politica = concedidas is null ? null : await politicas.Include(x => x.Pacote)
        .SingleAsync(x => x.PacoteAcessoId == concedidas.PacoteAcessoId, cancellationToken);

      validaAte = concedidas?.ValidaAte;
    }

    if (politica?.LimiteQuantidade is not int limite)
      throw new ErroAplicacaoException("BUSCA_NAO_PERMITIDA", "Sua conta não possui acesso à busca.", TipoErro.AcessoNegado);

    var inicio = new DateTimeOffset(agora.UtcDateTime.Date, TimeSpan.Zero);

    var utilizado = await db.RegistrosUso.AsNoTracking().Where(x => x.RecursoCodigo == RecursoBusca
      && x.ContaId == ator.ContaId && x.VisitanteId == ator.VisitanteId && x.OcorridoEm >= inicio)
      .SumAsync(x => x.Quantidade, cancellationToken);

    return new(ator.VisitanteId.HasValue ? "Visitante" : politica.Pacote.Codigo == "FREE" ? "Free" : "Pago",
      politica.Pacote.Codigo, limite, utilizado, Math.Max(0, limite - utilizado), inicio.AddDays(1), validaAte);
  }

  public async Task RegistrarAsync(AtorBusca ator, Guid buscaId, DbConnection conexao, DbTransaction transacao, CancellationToken cancellationToken)
  {
    // Cada contexto continua acessando apenas seu schema. O monólito compartilha a transação de aceitação.
    db.Database.SetDbConnection(conexao, contextOwnsConnection: false);

    await db.Database.UseTransactionAsync(transacao, cancellationToken);

    try
    {
      await db.Database.ExecuteSqlInterpolatedAsync(
        $"SELECT pg_advisory_xact_lock(hashtextextended({ator.Chave}, 710003))", cancellationToken);

      var acesso = await ObterAsync(ator, cancellationToken);

      if (acesso.Restante == 0)
        throw new ErroAplicacaoException("LIMITE_BUSCAS", "Limite diário de buscas atingido.", TipoErro.LimiteExcedido);

      db.RegistrosUso.Add(new RegistroUso(RecursoBusca, 1, DateTimeOffset.UtcNow, ator.ContaId, ator.VisitanteId, buscaId));

      await db.SaveChangesAsync(cancellationToken);
    }
    finally
    {
      await db.Database.UseTransactionAsync((DbTransaction?)null, CancellationToken.None);
    }
  }
}
