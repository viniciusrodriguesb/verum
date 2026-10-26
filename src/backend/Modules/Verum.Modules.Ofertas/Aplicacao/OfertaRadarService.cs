using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Verum.Modules.Ofertas.Contratos.Descoberta;
using Verum.Modules.Ofertas.Contratos.Descoberta.DTOs;
using Verum.Modules.Ofertas.Contratos.Radar;
using Verum.Modules.Ofertas.Contratos.Radar.DTOs;
using Verum.Modules.Ofertas.Dominio;
using Verum.Modules.Ofertas.Infraestrutura.Descoberta.Playwright.Configuracoes;
using Verum.Modules.Ofertas.Infraestrutura.Persistencia;

namespace Verum.Modules.Ofertas.Aplicacao;

internal sealed class OfertaRadarService(OfertasDbContext db, IConsultaLoja consulta,
  IOfertasDescobertaService ofertas, IOptions<PlaywrightOptions> options) : IOfertaRadarService
{
  public async Task<OfertaRadar?> ObterAsync(Guid ofertaId, CancellationToken cancellationToken)
  {
    return await db.Observacoes.AsNoTracking().Where(x => x.OfertaId == ofertaId
      && x.Oferta.Status == StatusOferta.Ativa && x.Oferta.Loja.Status == StatusLoja.Ativa
      && x.Fonte.Ativa && x.Oferta.Disponibilidade != DisponibilidadeOferta.Indisponivel
      && x.ObservadaEm <= DateTimeOffset.UtcNow)
      .OrderByDescending(x => x.ObservadaEm).ThenByDescending(x => x.Id)
      .Select(x => new OfertaRadar(x.OfertaId, x.Oferta.ProdutoVarianteId, x.Id, x.Oferta.TituloExterno,
        x.Oferta.Url, x.Preco, x.CondicaoPreco ?? "Confirme na loja.", x.Fonte.Codigo,
        x.Oferta.ImagemUrl, x.ObservadaEm, x.Oferta.ValidaAte)).FirstOrDefaultAsync(cancellationToken);
  }

  public async Task<OfertaRadar?> AtualizarAsync(Guid ofertaId, CancellationToken cancellationToken)
  {
    var atual = await ObterAsync(ofertaId, cancellationToken);

    if (atual is null || atual.ValidaAte > DateTimeOffset.UtcNow) return atual;

    if (!consulta.Disponivel || !options.Value.Lojas.ContainsKey(atual.Fonte)) return null;

    // Mesmo executor e seletores da Busca. Identidade provisória por anúncio: nunca troca de variante.
    var termo = string.Join(' ', atual.Nome.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(8));

    var resultado = await consulta.ConsultarAsync(atual.Fonte, termo, cancellationToken);

    var item = resultado.Itens.FirstOrDefault(x => x.Url == atual.Url);

    if (item is null || !item.Campos.TryGetValue("nome", out var nome)
      || !string.Equals(nome?.Trim(), atual.Nome.Trim(), StringComparison.OrdinalIgnoreCase)) return null;

    item.Campos.TryGetValue("precoPix", out var texto);

    var pix = !string.IsNullOrWhiteSpace(texto);

    if (!pix) item.Campos.TryGetValue("preco", out texto);

    if (!NormalizacaoPreco.Tentar(texto, out var preco) || preco <= 0
      || atual.Condicao.Contains("PIX", StringComparison.OrdinalIgnoreCase) != pix) return null;

    await ofertas.RegistrarAsync(atual.VarianteId, new OfertaDescoberta(atual.Nome, atual.Url, preco,
      atual.Condicao,
      atual.ImagemUrl, atual.Fonte, false, "Anúncio reencontrado pelo executor compartilhado.", resultado.ConsultadaEm), cancellationToken);

    return await ObterAsync(ofertaId, cancellationToken);
  }
}
