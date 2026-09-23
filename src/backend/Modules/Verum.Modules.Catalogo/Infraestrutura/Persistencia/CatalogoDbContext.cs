using Microsoft.EntityFrameworkCore;
using Verum.Modules.Catalogo.Dominio;

namespace Verum.Modules.Catalogo.Infraestrutura.Persistencia;

internal sealed class CatalogoDbContext(DbContextOptions<CatalogoDbContext> options) : DbContext(options)
{
  public DbSet<Categoria> Categorias => Set<Categoria>();

  public DbSet<Marca> Marcas => Set<Marca>();

  public DbSet<Produto> Produtos => Set<Produto>();

  public DbSet<ProdutoVariante> Variantes => Set<ProdutoVariante>();

  public DbSet<ProdutoIdentificador> Identificadores => Set<ProdutoIdentificador>();

  public DbSet<ProdutoTermoBusca> TermosBusca => Set<ProdutoTermoBusca>();

  public DbSet<ProdutoImagem> Imagens => Set<ProdutoImagem>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.HasDefaultSchema("catalogo");

    modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogoDbContext).Assembly);
  }
}
