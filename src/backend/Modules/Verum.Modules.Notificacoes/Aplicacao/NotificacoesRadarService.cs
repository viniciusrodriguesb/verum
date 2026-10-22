using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Verum.Modules.Contas.Contratos;
using Verum.Modules.Notificacoes.Aplicacao.Configuracoes;
using Verum.Modules.Notificacoes.Contratos;
using Verum.Modules.Notificacoes.Contratos.DTOs;
using Verum.Modules.Notificacoes.Dominio;
using Verum.Modules.Notificacoes.Infraestrutura.Persistencia;
using Verum.Modules.Ofertas.Contratos.Radar;

namespace Verum.Modules.Notificacoes.Aplicacao;

internal sealed class NotificacoesRadarService(NotificacoesDbContext db, IOptions<EmailOptions> options,
  IContatoContaService contas, IEnvioEmail envio, IOfertaRadarService ofertas,
  ILogger<NotificacoesRadarService> logger) : INotificacoesRadarService
{
  public bool EmailDisponivel => options.Value.Habilitado;

  public async Task<PreferenciasAviso> PreferenciasAsync(Guid contaId, CancellationToken cancellationToken)
  {
    var preferencia = await db.Preferencias.AsNoTracking()
      .SingleOrDefaultAsync(x => x.ContaId == contaId && x.Canal == CanalNotificacao.Email, cancellationToken);

    return preferencia is null ? new(false, 22, 8, "America/Sao_Paulo")
      : new(preferencia.Habilitada, preferencia.SilencioInicio, preferencia.SilencioFim, preferencia.Fuso);
  }

  public async Task SalvarPreferenciasAsync(Guid contaId, PreferenciasAviso pedido, CancellationToken cancellationToken)
  {
    await using var transacao = await db.Database.BeginTransactionAsync(cancellationToken);

    await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({contaId.ToString()}, 710022))", cancellationToken);

    var preferencia = await db.Preferencias.SingleOrDefaultAsync(x => x.ContaId == contaId
      && x.Canal == CanalNotificacao.Email, cancellationToken);

    if (preferencia is null)
    {
      preferencia = new PreferenciaNotificacao(contaId, CanalNotificacao.Email, pedido.EmailHabilitado);

      db.Preferencias.Add(preferencia);
    }

    preferencia.Atualizar(pedido.EmailHabilitado, pedido.SilencioInicio, pedido.SilencioFim, pedido.Fuso);

    await db.SaveChangesAsync(cancellationToken);

    await transacao.CommitAsync(cancellationToken);
  }

  public async Task RegistrarAsync(AvisoOportunidade aviso, CancellationToken cancellationToken)
  {
    await using var transacao = await db.Database.BeginTransactionAsync(cancellationToken);

    await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({aviso.Id.ToString()}, 710023))", cancellationToken);

    if (await db.Notificacoes.AnyAsync(x => x.Id == aviso.Id, cancellationToken)) return;

    var mensagem = $"{aviso.Nome}: R$ {aviso.Preco:F2}. Preço do produto; frete não incluído. Confira as condições e o preço atual na loja. A checagem é periódica.";

    db.Notificacoes.Add(new Notificacao(aviso.ContaId, TipoNotificacao.OportunidadeDetectada,
      "Sua meta de preço foi atingida", mensagem, JsonSerializer.Serialize(aviso), aviso.Id));

    db.Entregas.Add(new EntregaNotificacao(aviso.Id, CanalNotificacao.Email));

    await db.SaveChangesAsync(cancellationToken);

    await transacao.CommitAsync(cancellationToken);
  }

  public async Task<IReadOnlyList<NotificacaoConsulta>> ListarAsync(Guid contaId, CancellationToken cancellationToken)
  {
    var lista = await db.Notificacoes.AsNoTracking().Include(x => x.Entregas).Where(x => x.ContaId == contaId)
      .OrderByDescending(x => x.CriadaEm).Take(50).ToArrayAsync(cancellationToken);

    return lista.Select(x => new NotificacaoConsulta(x.Id, x.Titulo, x.Mensagem, x.CriadaEm,
      x.Tipo == TipoNotificacao.OportunidadeDetectada ? JsonSerializer.Deserialize<AvisoOportunidade>(x.Dados!)?.Url : null,
      x.Entregas.Select(y => new EntregaConsulta(y.Status.ToString(), y.QuantidadeTentativas, y.CodigoErro)).ToArray())).ToArray();
  }

  public async Task EnviarPendentesAsync(CancellationToken cancellationToken)
  {
    if (!EmailDisponivel) return;

    await db.Database.OpenConnectionAsync(cancellationToken);

    try
    {
      // Lease de sessão: reinício libera o lock, preservando entregas ainda não confirmadas.
      await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_lock(710024)", cancellationToken);

      var agora = DateTimeOffset.UtcNow;

      var entregas = await db.Entregas.Include(x => x.Notificacao)
        .Where(x => (x.Status == StatusEntrega.Pendente || x.Status == StatusEntrega.AguardandoNovaTentativa)
          && (x.ProximaTentativaEm == null || x.ProximaTentativaEm <= agora)).OrderBy(x => x.Id).Take(20).ToArrayAsync(cancellationToken);

      foreach (var entrega in entregas)
        await EnviarAsync(entrega, cancellationToken);
    }
    finally
    {
      await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_unlock(710024)", CancellationToken.None);

      await db.Database.CloseConnectionAsync();
    }
  }

  private async Task EnviarAsync(EntregaNotificacao entrega, CancellationToken cancellationToken)
  {
    var agora = DateTimeOffset.UtcNow;

    var aviso = JsonSerializer.Deserialize<AvisoOportunidade>(entrega.Notificacao.Dados!);

    var preferencia = await PreferenciasAsync(entrega.Notificacao.ContaId, cancellationToken);

    var email = await contas.EmailAtivoAsync(entrega.Notificacao.ContaId, cancellationToken);

    var atual = aviso is null ? null : await ofertas.ObterAsync(aviso.OfertaId, cancellationToken);

    if (aviso is null || aviso.ExpiraEm <= agora || atual is null || atual.ValidaAte <= agora
      || atual.Preco > aviso.Preco || !preferencia.EmailHabilitado || email is null)
      entrega.Cancelar();
    else if (EmSilencio(preferencia, agora))
      entrega.Adiar(agora.AddMinutes(15));
    else
    {
      try
      {
        await envio.EnviarAsync(entrega.NotificacaoId, email, entrega.Notificacao.Titulo,
          entrega.Notificacao.Mensagem + "\n\n" + aviso.Url, cancellationToken);

        entrega.Entregar(DateTimeOffset.UtcNow);
      }
      catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
      {
        logger.LogWarning("Falha no envio {NotificacaoId}: {TipoErro}", entrega.NotificacaoId, exception.GetType().Name);

        entrega.Falhar(DateTimeOffset.UtcNow, options.Value.MaximoTentativas);
      }
    }

    await db.SaveChangesAsync(cancellationToken);
  }

  internal static bool EmSilencio(PreferenciasAviso preferencia, DateTimeOffset agora)
  {
    var hora = TimeZoneInfo.ConvertTime(agora, TimeZoneInfo.FindSystemTimeZoneById(preferencia.Fuso)).Hour;

    return preferencia.SilencioInicio == preferencia.SilencioFim ? false
      : preferencia.SilencioInicio < preferencia.SilencioFim
        ? hora >= preferencia.SilencioInicio && hora < preferencia.SilencioFim
        : hora >= preferencia.SilencioInicio || hora < preferencia.SilencioFim;
  }
}
