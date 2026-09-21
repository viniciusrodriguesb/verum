using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Catalogo.Dominio.ProdutoImagem;

namespace Verum.Modules.Catalogo.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class ProdutoImagemMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("produto_imagem", table =>
    {
      table.HasCheckConstraint("ck_produto_imagem_id", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_produto_imagem_produto_id", "produto_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_produto_imagem_produto_variante_id", "produto_variante_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_produto_imagem_url", "length(btrim(url)) > 0");
      table.HasCheckConstraint("ck_produto_imagem_origem", "length(btrim(origem)) > 0");
      table.HasCheckConstraint("ck_produto_imagem_ordem", "ordem >= 0 AND ordem <= 32767");
    });
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
    builder.Property(x => x.ProdutoId).HasColumnName("produto_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.ProdutoVarianteId).HasColumnName("produto_variante_id").HasColumnType("uuid");
    builder.Property(x => x.Url).HasColumnName("url").HasColumnType("text").IsRequired();
    builder.Property(x => x.Origem).HasColumnName("origem").HasMaxLength(100).HasColumnType("varchar(100)").IsRequired();
    builder.Property(x => x.Principal).HasColumnName("principal").HasColumnType("boolean").IsRequired();
    builder.Property(x => x.Ordem).HasColumnName("ordem").HasColumnType("smallint").IsRequired();
    builder.Property(x => x.CriadaEm).HasColumnName("criada_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.HasOne(x => x.Produto).WithMany(x => x.Imagens).HasForeignKey(x => x.ProdutoId).OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.Variante).WithMany(x => x.Imagens)
      .HasForeignKey(x => new { x.ProdutoVarianteId, x.ProdutoId })
      .HasPrincipalKey(x => new { x.Id, x.ProdutoId }).OnDelete(DeleteBehavior.Restrict);
  }
}
