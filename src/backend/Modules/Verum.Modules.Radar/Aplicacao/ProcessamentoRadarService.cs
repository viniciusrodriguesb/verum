using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Verum.Modules.Acesso.Contratos;
using Verum.Modules.Notificacoes.Contratos;
using Verum.Modules.Notificacoes.Contratos.DTOs;
using Verum.Modules.Ofertas.Contratos.Radar;
using Verum.Modules.Ofertas.Contratos.Radar.DTOs;
using Verum.Modules.Radar.Contratos;
using Verum.Modules.Radar.Contratos.Configuracoes;
using Verum.Modules.Radar.Dominio;
using Verum.Modules.Radar.Infraestrutura.Persistencia;

namespace Verum.Modules.Radar.Aplicacao;

internal sealed class ProcessamentoRadarService(RadarDbContext db, IAcessoRadarService acesso,
  IOfertaRadarService ofertas, INotificacoesRadarService notificacoes, IOptions<RadarOptions> options,
  ILogger<ProcessamentoRadarService> logger) : IProcessamentoRadarService
{
  public async Task<IReadOnlyList<Guid>> PendentesAsync(CancellationToken cancellationToken)
  {
    if (!options.Value.Habilitado) return [];

    var agora = DateTimeOffset.UtcNow;

    return await db.Monitoramentos.AsNoTracking().Where(x => x.Status == StatusMonitoramento.Ativo
      && (x.ProximaVerificacaoEm == null || x.ProximaVerificacaoEm <= agora) && x.OfertaId != Guid.Empty)
      .OrderBy(x => x.ProximaVerificacaoEm).Take(options.Value.Lote).Select(x => x.Id).ToArrayAsync(cancellationToken);
  }

  public async Task VerificarAsync(Guid monitoramentoId, CancellationToken cancellationToken)
  {
    if (!options.Value.Habilitado) return;

    var origem = await db.Monitoramentos.AsNoTracking().SingleOrDefaultAsync(x => x.Id == monitoramentoId, cancellationToken);

    if (origem is null || origem.Status != StatusMonitoramento.Ativo) return;

    await db.Database.OpenConnectionAsync(cancellationToken);

    try
    {
      // Uma verificação por anúncio/variante. Nenhuma transação fica aberta durante I/O externo.
      await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_lock(hashtextextended({origem.OfertaId.ToString()}, 710021))", cancellationToken);

      var agora = DateTimeOffset.UtcNow;

      var grupo = await db.Monitoramentos.Where(x => x.OfertaId == origem.OfertaId && x.Status == StatusMonitoramento.Ativo
        && (x.ProximaVerificacaoEm == null || x.ProximaVerificacaoEm <= agora)).ToArrayAsync(cancellationToken);

      var elegiveis = new List<Monitoramento>();

      foreach (var item in grupo)
      {
        if (await acesso.PermitidoAsync(item.ContaId, cancellationToken)) elegiveis.Add(item);
        else item.Avaliar(null, null, null, agora, options.Value.IntervaloVerificacaoMinutos, options.Value.CooldownMinutos, options.Value.HisteresePercentual);
      }

      OfertaRadar? oferta = null;

      if (elegiveis.Count > 0)
      {
        try { oferta = await ofertas.AtualizarAsync(origem.OfertaId, cancellationToken); }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
          logger.LogWarning("Verificação indisponível para {OfertaId}: {TipoErro}", origem.OfertaId, exception.GetType().Name);
        }
      }

      foreach (var item in elegiveis)
        Avaliar(item, oferta, agora);

      // Alteração da meta e oportunidade são atômicas no schema Radar.
      await db.SaveChangesAsync(cancellationToken);
    }
    finally
    {
      await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_unlock(hashtextextended({origem.OfertaId.ToString()}, 710021))", CancellationToken.None);

      await db.Database.CloseConnectionAsync();
    }
  }

  private void Avaliar(Monitoramento item, OfertaRadar? oferta, DateTimeOffset agora)
  {
    if (oferta is not null && (oferta.VarianteId != item.ProdutoVarianteId || oferta.ValidaAte <= agora
      || !string.Equals(oferta.Nome.Trim(), item.Nome.Trim(), StringComparison.OrdinalIgnoreCase)
      || oferta.ObservadaEm > agora || oferta.Condicao != item.Condicao)) oferta = null;

    if (!item.Avaliar(oferta?.Preco, oferta?.Id, oferta?.ObservacaoId, agora,
      options.Value.IntervaloVerificacaoMinutos, options.Value.CooldownMinutos, options.Value.HisteresePercentual)) return;

    db.Oportunidades.Add(new Oportunidade(item.Id, oferta!.Id, oferta.ObservacaoId, oferta.Preco,
      item.PrecoAlvo, JsonSerializer.Serialize(oferta), Math.Max(0, item.PrecoInicial - oferta.Preco),
      decimal.Round(Math.Max(0, (item.PrecoInicial - oferta.Preco) / item.PrecoInicial * 100m), 4), oferta.ValidaAte));
  }

  public async Task EncaminharAvisosAsync(CancellationToken cancellationToken)
  {
    var pendentes = await db.Oportunidades.Include(x => x.Monitoramento).Where(x => x.AvisoEncaminhadoEm == null)
      .OrderBy(x => x.DetectadaEm).Take(options.Value.Lote).ToArrayAsync(cancellationToken);

    foreach (var item in pendentes)
    {
      var oferta = JsonSerializer.Deserialize<OfertaRadar>(item.OfertaSnapshot)!;

      // Repete com a mesma chave após queda. Notificações registra uma única vez, em transação própria.
      await notificacoes.RegistrarAsync(new AvisoOportunidade(item.Id, item.Monitoramento.ContaId,
        item.Monitoramento.Nome, item.PrecoObservado, oferta.Url, item.ExpiraEm!.Value, item.OfertaId), cancellationToken);

      item.ConfirmarEncaminhamento();

      await db.SaveChangesAsync(cancellationToken);
    }
  }
}
