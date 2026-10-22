using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Verum.Modules.Catalogo.Contratos;
using Verum.Modules.Catalogo.Dominio;
using Verum.Modules.Catalogo.Infraestrutura.Persistencia;

namespace Verum.Modules.Catalogo.Aplicacao;

internal sealed class CatalogoDescobertaService(CatalogoDbContext db) : ICatalogoDescobertaService
{
  public Task<Guid?> ObterProdutoAsync(Guid varianteId, CancellationToken cancellationToken) =>
    db.Variantes.AsNoTracking().Where(x => x.Id == varianteId && x.Status == StatusProduto.Ativo)
      .Select(x => (Guid?)x.ProdutoId).SingleOrDefaultAsync(cancellationToken);

  public Task<Guid[]> ResolverAsync(string consulta, CancellationToken cancellationToken) =>
    db.TermosBusca.AsNoTracking()
      .Where(x => x.TermoNormalizado == consulta.ToLowerInvariant() && x.ProdutoVarianteId != null
        && x.Produto.Status == StatusProduto.Ativo)
      .Select(x => x.ProdutoVarianteId!.Value)
      .Distinct()
      .ToArrayAsync(cancellationToken);

  public async Task<Guid> RegistrarAsync(string consulta, string nome, string url, CancellationToken cancellationToken)
  {
    var chave = "fonte-" + Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(url)));

    await using var transacao = await db.Database.BeginTransactionAsync(cancellationToken);

    // Curta exclusão de escrita no módulo; nenhuma chamada externa dentro da transação.
    await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(710001)", cancellationToken);

    var variante = await db.Variantes.SingleOrDefaultAsync(x => x.Slug == chave, cancellationToken);

    if (variante is null)
    {
      var categoria = await db.Categorias.SingleOrDefaultAsync(x => x.Slug == "nao-classificado", cancellationToken);

      if (categoria is null)
      {
        categoria = new Categoria("Não classificado", "nao-classificado");

        db.Categorias.Add(categoria);
      }

      var marca = await db.Marcas.SingleOrDefaultAsync(x => x.Slug == "nao-informada", cancellationToken);

      if (marca is null)
      {
        marca = new Marca("Não informada", "nao-informada");

        db.Marcas.Add(marca);
      }

      // Identidade provisória por URL: não afirma equivalência entre anúncios ou variantes.
      var produto = new Produto(categoria.Id, marca.Id, nome, chave, "{}");

      variante = new ProdutoVariante(produto.Id, nome, chave, "{}");

      db.Produtos.Add(produto);

      db.Variantes.Add(variante);
    }

    if (!await db.TermosBusca.AnyAsync(x => x.ProdutoVarianteId == variante.Id
      && x.TermoNormalizado == consulta.ToLowerInvariant(), cancellationToken))
      db.TermosBusca.Add(new ProdutoTermoBusca(variante.ProdutoId, consulta, OrigemTermo.Fonte, 0.8m, variante.Id));

    await db.SaveChangesAsync(cancellationToken);

    await transacao.CommitAsync(cancellationToken);

    return variante.Id;
  }
}
