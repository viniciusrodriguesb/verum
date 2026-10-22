using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Verum.Modules.Busca.Aplicacao.Descoberta.Configuracoes;
using Verum.Modules.Busca.Contratos;
using Verum.Modules.Busca.Contratos.Cache;
using Verum.Modules.Busca.Contratos.DTOs;
using Verum.Modules.Busca.Dominio;
using Verum.Modules.Busca.Infraestrutura.Persistencia;
using Verum.Modules.Catalogo.Contratos;
using Verum.Modules.Ofertas.Contratos.Descoberta;
using Verum.Modules.Ofertas.Contratos.Descoberta.DTOs;

namespace Verum.Modules.Busca.Aplicacao.Descoberta;

internal sealed class ProcessamentoBuscaService(
  BuscaDbContext db, ICatalogoDescobertaService catalogo, IOfertasDescobertaService ofertas,
  ConsultaFontesService fontes, IOptions<BuscaOptions> options, IBuscaCache cache,
  ILogger<ProcessamentoBuscaService> logger) : IProcessamentoBuscaService
{
  public async Task ProcessarAsync(Guid buscaId, CancellationToken cancellationToken)
  {
    if (buscaId == Guid.Empty) throw new ArgumentException("BuscaId deve ser informado.");

    using var log = logger.BeginScope(new Dictionary<string, object> { ["BuscaId"] = buscaId });

    await db.Database.OpenConnectionAsync(cancellationToken);

    try
    {
      // Lock de sessão, não transação: impede execuções simultâneas do mesmo ID entre workers.
      // A conexão é devolvida ao final; não mantém transação aberta durante consultas externas.
      await db.Database.ExecuteSqlInterpolatedAsync(
        $"SELECT pg_advisory_lock(hashtextextended({buscaId.ToString()}, 710004))", cancellationToken);

      db.ChangeTracker.Clear();

      var busca = await db.Buscas.SingleOrDefaultAsync(x => x.Id == buscaId, cancellationToken)
        ?? throw new InvalidOperationException("Mensagem referencia uma busca inexistente.");

      if (busca.ConcluidaEm.HasValue) return;

      var ofertasAtuais = new List<OfertaConsultada>();

      var diagnosticos = new List<DiagnosticoFonte>();

      var expirada = busca.ExpiraEm <= DateTimeOffset.UtcNow;

      var consultaEspecifica = NormalizacaoOferta.ConsultaEspecifica(busca.ConsultaOriginal);

      busca.Iniciar();

      await db.SaveChangesAsync(cancellationToken);

      if (!expirada && consultaEspecifica)
        expirada = await ColetarOfertasAsync(busca, ofertasAtuais, diagnosticos, cancellationToken);

      cancellationToken.ThrowIfCancellationRequested();

      foreach (var diagnostico in diagnosticos)
        await ofertas.RegistrarExecucaoAsync(buscaId, diagnostico, cancellationToken);

      var fundamentadas = await ofertas.ObterFundamentosRankingAsync(ofertasAtuais, cancellationToken);

      var ranking = RankingOfertas.Classificar(fundamentadas, options.Value.MaximoResultados, busca.ConsultaOriginal);

      await SalvarResultadoAsync(busca, ofertasAtuais.Count, ranking, diagnosticos, expirada, consultaEspecifica, cancellationToken);

      await cache.InvalidarAsync(buscaId, cancellationToken);

      logger.LogInformation("Busca finalizada: {Status}, {Ofertas} ofertas, parcial {Parcial}.", busca.Status, ranking.Count, busca.ResultadoParcial);
    }
    finally
    {
      try
      {
        await db.Database.ExecuteSqlInterpolatedAsync(
          $"SELECT pg_advisory_unlock(hashtextextended({buscaId.ToString()}, 710004))", CancellationToken.None);
      }
      finally
      {
        await db.Database.CloseConnectionAsync();
      }
    }
  }

  private async Task<bool> ColetarOfertasAsync(Dominio.Busca busca, List<OfertaConsultada> ofertasAtuais, List<DiagnosticoFonte> diagnosticos, CancellationToken cancellationToken)
  {
    var expirada = false;

    using var prazo = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

    var tempoRestante = busca.ExpiraEm - DateTimeOffset.UtcNow;

    prazo.CancelAfter(tempoRestante > TimeSpan.Zero ? tempoRestante : TimeSpan.FromMilliseconds(1));

    try
    {
      var variantes = await catalogo.ResolverAsync(busca.ConsultaNormalizada, prazo.Token);

      ofertasAtuais.AddRange((await ofertas.ObterAtuaisAsync(variantes, prazo.Token))
        .Where(x => NormalizacaoOferta.Relevancia(busca.ConsultaOriginal, x.Nome) > 30m));

      var etapaCatalogo = new BuscaEtapa(busca.Id, EtapaBusca.Catalogo);

      etapaCatalogo.Finalizar(true);

      db.Etapas.Add(etapaCatalogo);

      busca.Avancar(EtapaBusca.Ofertas);

      await db.SaveChangesAsync(cancellationToken);

      if (ofertasAtuais.Count < options.Value.MinimoOfertas)
      {
        var coleta = await fontes.ConsultarAsync(busca.ConsultaOriginal, ofertasAtuais.Count, prazo.Token, cancellationToken);

        diagnosticos.AddRange(coleta.Fontes);

        expirada = prazo.IsCancellationRequested;

        foreach (var item in coleta.Ofertas)
        {
          var variante = await catalogo.RegistrarAsync(busca.ConsultaNormalizada, item.Nome, item.Url, cancellationToken);

          var oferta = await ofertas.RegistrarAsync(variante, item, cancellationToken);

          ofertasAtuais.RemoveAll(x => x.Url == oferta.Url);

          ofertasAtuais.Add(oferta);
        }
      }
    }
    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
    {
      expirada = true;
    }

    return expirada;
  }

  private async Task SalvarResultadoAsync(
    Dominio.Busca busca, int quantidadeEncontrada, IReadOnlyList<OfertaResultado> ranking,
    IReadOnlyList<DiagnosticoFonte> diagnosticos, bool expirada, bool consultaEspecifica,
    CancellationToken cancellationToken)
  {
    var parcial = expirada || diagnosticos.Any(x => x.Status != "Concluida" || x.Codigo is not null);

    await using (var transacao = await db.Database.BeginTransactionAsync(cancellationToken))
    {
      var resultado = new ResultadoBusca(busca.Id, RankingOfertas.Versao, quantidadeEncontrada, ranking.Count, parcial);

      db.Resultados.Add(resultado);

      foreach (var item in ranking)
        db.OfertasResultado.Add(new ResultadoOferta(resultado.Id, item.OfertaId, item.VarianteId, (short)item.Posicao,
          ClassificacaoOferta.Alternativa, item.Pontuacao, item.PontuacaoPreco, item.PontuacaoConfianca,
          item.PontuacaoAtualizacao, item.Explicacao, JsonSerializer.Serialize(item)));

      var detalhesFontes = JsonSerializer.Serialize(new { fontes = diagnosticos });

      var etapa = new BuscaEtapa(busca.Id, EtapaBusca.Ofertas, detalhesFontes);

      etapa.Finalizar(!expirada, detalhesFontes);

      db.Etapas.Add(etapa);

      var etapaRanking = new BuscaEtapa(busca.Id, EtapaBusca.Ranking);

      etapaRanking.Finalizar(true);

      db.Etapas.Add(etapaRanking);

      var etapaResultado = new BuscaEtapa(busca.Id, EtapaBusca.Resultado);

      etapaResultado.Finalizar(true);

      db.Etapas.Add(etapaResultado);

      busca.Finalizar(ranking.Count, parcial, diagnosticos.Count(x => x.Status == "Concluida"),
        diagnosticos.Count(x => x.Status == "Falhou"), expirada,
        !consultaEspecifica ? MotivoBuscaSemResultado.ConsultaInsuficiente : null);

      await db.SaveChangesAsync(cancellationToken);

      await transacao.CommitAsync(cancellationToken);
    }
  }
}
