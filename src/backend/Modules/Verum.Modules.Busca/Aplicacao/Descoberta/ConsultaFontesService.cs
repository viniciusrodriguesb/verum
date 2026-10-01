using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Verum.Modules.Busca.Contratos.InteligenciaArtificial;
using Verum.Modules.Ofertas.Contratos.Descoberta;

namespace Verum.Modules.Busca.Aplicacao.Descoberta;

internal sealed record ColetaOfertas(IReadOnlyList<OfertaDescoberta> Ofertas, IReadOnlyList<DiagnosticoFonte> Fontes);

internal sealed class ConsultaFontesService(
  IConsultaLoja lojas, IDescobertaProdutosIaService ia, IOptions<BuscaOptions> options,
  ILogger<ConsultaFontesService> logger)
{
  public async Task<ColetaOfertas> ConsultarAsync(string consulta, int quantidadeExistente, CancellationToken cancellationToken, CancellationToken cancelamentoHost = default)
  {
    var config = options.Value;

    var coletas = await Task.WhenAll(config.Lojas.Distinct(StringComparer.OrdinalIgnoreCase)
      .Select(fonte => ConsultarLojaAsync(fonte, consulta, cancellationToken, cancelamentoHost)));

    var itens = coletas.SelectMany(x => x.Itens).DistinctBy(x => x.Url).ToList();

    var fontes = coletas.Select(x => x.Diagnostico).ToList();

    if (quantidadeExistente + itens.Count < config.MinimoOfertas)
      await AdicionarFallbackIaAsync(consulta, itens, fontes, cancellationToken, cancelamentoHost);

    return new(itens.DistinctBy(x => x.Url).ToArray(), fontes);
  }

  private async Task<(List<OfertaDescoberta> Itens, DiagnosticoFonte Diagnostico)> ConsultarLojaAsync(
    string fonte, string consulta, CancellationToken cancellationToken, CancellationToken cancelamentoHost)
  {
    var inicio = DateTimeOffset.UtcNow;

    if (!lojas.Disponivel) return (Itens: new List<OfertaDescoberta>(), Diagnostico: new DiagnosticoFonte(
      fonte, false, "Ignorada", 0, 0, "DESATIVADA", inicio, DateTimeOffset.UtcNow));

    try
    {
      using var prazo = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

      prazo.CancelAfter(TimeSpan.FromSeconds(options.Value.TimeoutFonteSegundos));

      var resultado = await lojas.ConsultarAsync(fonte, consulta, prazo.Token).WaitAsync(prazo.Token);

      var validas = resultado.Itens.Select(x => NormalizacaoOferta.DaLoja(x, fonte, consulta, resultado.ConsultadaEm))
        .OfType<OfertaDescoberta>().ToList();

      return (Itens: validas, Diagnostico: new DiagnosticoFonte(fonte, false, "Concluida",
        resultado.Itens.Count + resultado.Descartados.Count, validas.Count,
        resultado.LimiteAtingido ? "LIMITE_ATINGIDO" : validas.Count < resultado.Itens.Count + resultado.Descartados.Count ? "CANDIDATOS_DESCARTADOS" : null,
        inicio, DateTimeOffset.UtcNow));
    }
    catch (Exception erro) when (!cancelamentoHost.IsCancellationRequested)
    {
      logger.LogWarning("Fonte {Fonte} indisponível: {TipoErro}.", fonte, erro.GetType().Name);

      return (Itens: new List<OfertaDescoberta>(), Diagnostico: new DiagnosticoFonte(fonte, false, "Falhou", 0, 0,
        erro is OperationCanceledException or TimeoutException ? "TIMEOUT" : "FONTE_INDISPONIVEL", inicio, DateTimeOffset.UtcNow));
    }
  }

  private async Task AdicionarFallbackIaAsync(
    string consulta, List<OfertaDescoberta> itens, List<DiagnosticoFonte> fontes,
    CancellationToken cancellationToken, CancellationToken cancelamentoHost)
  {
    var inicioIa = DateTimeOffset.UtcNow;

    if (!ia.Disponivel || cancellationToken.IsCancellationRequested)
    {
      fontes.Add(new("ia", true, "Ignorada", 0, 0, cancellationToken.IsCancellationRequested ? "PRAZO_ESGOTADO" : "IA_NAO_CONFIGURADA",
        inicioIa, DateTimeOffset.UtcNow));
    }
    else
    {
      try
      {
        using var prazo = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        prazo.CancelAfter(TimeSpan.FromSeconds(options.Value.TimeoutIaSegundos));

        var resposta = await ia.ConsultarAsync(consulta, prazo.Token).WaitAsync(prazo.Token);

        var validas = resposta.Resultado.Produtos.Select(x => NormalizacaoOferta.DaIa(x, consulta, DateTimeOffset.UtcNow))
          .OfType<OfertaDescoberta>().ToArray();

        itens.AddRange(validas);

        fontes.Add(new("ia", true, "Concluida", resposta.Resultado.Produtos.Length + resposta.Descartados.Count, validas.Length,
          validas.Length < resposta.Resultado.Produtos.Length + resposta.Descartados.Count ? "CANDIDATOS_DESCARTADOS" : null,
          inicioIa, DateTimeOffset.UtcNow));
      }
      catch (Exception erro) when (!cancelamentoHost.IsCancellationRequested)
      {
        logger.LogWarning("Fallback IA indisponível: {TipoErro}.", erro.GetType().Name);

        fontes.Add(new("ia", true, "Falhou", 0, 0,
          erro is OperationCanceledException or TimeoutException ? "TIMEOUT" : "IA_INDISPONIVEL", inicioIa, DateTimeOffset.UtcNow));
      }
    }
  }
}
