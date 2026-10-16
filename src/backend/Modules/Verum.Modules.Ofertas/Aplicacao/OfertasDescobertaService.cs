using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Verum.Modules.Ofertas.Contratos.Descoberta;
using Verum.Modules.Ofertas.Contratos.Descoberta.DTOs;
using Verum.Modules.Ofertas.Dominio;
using Verum.Modules.Ofertas.Infraestrutura.Persistencia;

namespace Verum.Modules.Ofertas.Aplicacao;

internal sealed class OfertasDescobertaService(OfertasDbContext db) : IOfertasDescobertaService
{
  public async Task<IReadOnlyList<OfertaConsultada>> ObterAtuaisAsync(Guid[] variantes, CancellationToken cancellationToken)
  {
    var agora = DateTimeOffset.UtcNow;

    var ofertas = await db.Ofertas.AsNoTracking().Include(x => x.Fonte).Include(x => x.Loja)
      .Where(x => variantes.Contains(x.ProdutoVarianteId) && x.ValidaAte > agora
        && x.Status == StatusOferta.Ativa && x.Loja.Status == StatusLoja.Ativa && x.Fonte.Ativa
        && x.Disponibilidade != DisponibilidadeOferta.Indisponivel)
      .ToArrayAsync(cancellationToken);

    return ofertas.Select(Converter).ToArray();
  }

  public async Task<OfertaConsultada> RegistrarAsync(Guid varianteId, OfertaDescoberta item, CancellationToken cancellationToken)
  {
    await using var transacao = await db.Database.BeginTransactionAsync(cancellationToken);

    await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(710002)", cancellationToken);

    var host = new Uri(item.Url).IdnHost;

    var loja = await db.Lojas.FirstOrDefaultAsync(x => x.Dominio == host, cancellationToken);

    if (loja is null)
    {
      loja = new Loja(host, 50m, host, "https://" + host);

      db.Lojas.Add(loja);
    }

    var fonte = await ObterFonteAsync(item.Fonte, item.OrigemIa, cancellationToken);

    var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(item.Url)));

    var oferta = await db.Ofertas.Include(x => x.Fonte).Include(x => x.Loja)
      .SingleOrDefaultAsync(x => x.LojaId == loja.Id && x.UrlHash == hash, cancellationToken);

    if (oferta is null)
    {
      oferta = new Oferta(varianteId, loja.Id, fonte.Id, item.Nome, item.Url, item.Preco,
        DisponibilidadeOferta.Desconhecida, CondicaoProduto.Desconhecida, item.ObservadaEm, item.ObservadaEm.AddMinutes(15),
        precoPix: item.CondicaoPreco.Contains("PIX", StringComparison.OrdinalIgnoreCase) ? item.Preco : null,
        condicaoPreco: item.CondicaoPreco, imagemUrl: item.ImagemUrl);

      db.Ofertas.Add(oferta);
    }
    else
    {
      oferta.AtualizarObservacao(item.Nome, fonte.Id, item.Preco, item.CondicaoPreco, item.ImagemUrl, item.ObservadaEm);
    }

    var hashObservacao = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(item))));

    if (!await db.Observacoes.AnyAsync(x => x.OfertaId == oferta.Id && x.HashConteudo == hashObservacao, cancellationToken))
      db.Observacoes.Add(new OfertaObservacao(oferta.Id, fonte.Id, item.Preco, oferta.Disponibilidade,
        item.ObservadaEm, hashObservacao, precoPix: oferta.PrecoPix, condicaoPreco: item.CondicaoPreco,
        evidencia: JsonSerializer.Serialize(new { texto = item.Evidencia, origem = item.Url })));

    await db.SaveChangesAsync(cancellationToken);

    await transacao.CommitAsync(cancellationToken);

    return new(oferta.Id, oferta.ProdutoVarianteId, oferta.TituloExterno, oferta.Url, oferta.PrecoAtual,
      oferta.CondicaoPreco!, oferta.ImagemUrl, fonte.Codigo, Math.Min(fonte.NivelConfianca, loja.PontuacaoConfianca),
      oferta.ObservadaEm, oferta.ValidaAte)
    {
      LojaId = loja.Id,

      ConfiancaLoja = loja.PontuacaoConfianca,

      ConfiancaDado = fonte.NivelConfianca
    };
  }

  public async Task RegistrarExecucaoAsync(Guid buscaId, DiagnosticoFonte item, CancellationToken cancellationToken)
  {
    await using var transacao = await db.Database.BeginTransactionAsync(cancellationToken);

    await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(710002)", cancellationToken);

    var fonte = await ObterFonteAsync(item.Fonte, item.OrigemIa, cancellationToken);

    var execucao = new ExecucaoConsultaFonte(buscaId, buscaId, fonte.Id, fonte.Tipo, item.IniciadaEm);

    execucao.Finalizar(item.Status == "Falhou" ? StatusConsultaFonte.Falhou
      : item.Status == "Ignorada" ? StatusConsultaFonte.Ignorada
      : item.Aceitos == 0 ? StatusConsultaFonte.ConcluidaSemResultado : StatusConsultaFonte.Concluida,
      item.Encontrados, item.Aceitos, item.Codigo, item.FinalizadaEm);

    db.ExecucoesConsultaFonte.Add(execucao);

    await db.SaveChangesAsync(cancellationToken);

    await transacao.CommitAsync(cancellationToken);
  }

  private async Task<FonteOferta> ObterFonteAsync(string codigo, bool ia, CancellationToken cancellationToken)
  {
    var fonte = await db.Fontes.SingleOrDefaultAsync(x => x.Codigo == codigo, cancellationToken);

    if (fonte is not null) return fonte;

    fonte = new FonteOferta(codigo, ia ? TipoFonte.WebSearch : TipoFonte.Scraper, codigo, ia ? 50m : 80m);

    db.Fontes.Add(fonte);

    return fonte;
  }

  public async Task<IReadOnlyList<OfertaConsultada>> ObterFundamentosRankingAsync(IReadOnlyList<OfertaConsultada> ofertas, CancellationToken cancellationToken)
  {
    var variantes = ofertas.Select(x => x.VarianteId).Distinct().ToArray();

    if (variantes.Length == 0) return [];

    var inicio = DateTimeOffset.UtcNow.AddDays(-30);

    var diario = await db.Observacoes.AsNoTracking()
      .Where(x => variantes.Contains(x.Oferta.ProdutoVarianteId) && x.ObservadaEm >= inicio && x.Preco > 0)
      .GroupBy(x => new
      {
        x.Oferta.ProdutoVarianteId,
        x.Oferta.LojaId,
        Ano = x.ObservadaEm.UtcDateTime.Year,
        Mes = x.ObservadaEm.UtcDateTime.Month,
        Dia = x.ObservadaEm.UtcDateTime.Day,
        Pix = x.PrecoPix != null
      })
      .Select(x => new { x.Key.ProdutoVarianteId, x.Key.LojaId, x.Key.Ano, x.Key.Mes, x.Key.Dia, x.Key.Pix, Preco = x.Min(y => y.Preco) })
      .ToArrayAsync(cancellationToken);

    return ofertas.Select(x => x with
    {
      Historico = diario.Where(y => y.ProdutoVarianteId == x.VarianteId)
        .Select(y => new ObservacaoPrecoRanking(y.LojaId, new DateTimeOffset(y.Ano, y.Mes, y.Dia, 0, 0, 0, TimeSpan.Zero), y.Preco, y.Pix)).ToArray()
    }).ToArray();
  }

  private static OfertaConsultada Converter(Oferta x) =>
    new(x.Id, x.ProdutoVarianteId, x.TituloExterno, x.Url, x.PrecoAtual, x.CondicaoPreco ?? "Confirme na loja.",
      x.ImagemUrl, x.Fonte.Codigo, Math.Min(x.Fonte.NivelConfianca, x.Loja.PontuacaoConfianca), x.ObservadaEm, x.ValidaAte)
    {
      LojaId = x.LojaId,

      ConfiancaLoja = x.Loja.PontuacaoConfianca,

      ConfiancaDado = x.Fonte.NivelConfianca
    };
}
