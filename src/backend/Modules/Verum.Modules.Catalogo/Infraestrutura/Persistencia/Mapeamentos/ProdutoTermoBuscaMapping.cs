using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Catalogo.Dominio.ProdutoTermoBusca;

namespace Verum.Modules.Catalogo.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class ProdutoTermoBuscaMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("produto_termo_busca", table =>
    {
      table.HasCheckConstraint("ck_produto_termo_busca_id", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_produto_termo_busca_produto_id", "produto_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_produto_termo_busca_produto_variante_id", "produto_variante_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_produto_termo_busca_termo_original", "length(btrim(termo_original)) > 0");
      table.HasCheckConstraint("ck_produto_termo_busca_termo_normalizado", "length(btrim(termo_normalizado)) > 0");
      table.HasCheckConstraint("ck_produto_termo_busca_origem", "origem IN (1, 2, 3, 4)");
      table.HasCheckConstraint("ck_produto_termo_busca_confianca", "confianca >= 0 AND confianca <= 1");
    });
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
    builder.Property(x => x.ProdutoId).HasColumnName("produto_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.ProdutoVarianteId).HasColumnName("produto_variante_id").HasColumnType("uuid");
    builder.Property(x => x.TermoOriginal).HasColumnName("termo_original").HasMaxLength(300).HasColumnType("varchar(300)").IsRequired();
    builder.Property(x => x.TermoNormalizado).HasColumnName("termo_normalizado").HasMaxLength(300).HasColumnType("varchar(300)").IsRequired();
    builder.Property(x => x.Origem).HasColumnName("origem").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.Confianca).HasColumnName("confianca").HasPrecision(5, 4).HasColumnType("numeric(5,4)").IsRequired();
    builder.Property(x => x.CriadoEm).HasColumnName("criado_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.HasOne(x => x.Produto).WithMany(x => x.TermosBusca).HasForeignKey(x => x.ProdutoId).OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.Variante).WithMany(x => x.TermosBusca)
      .HasForeignKey(x => new { x.ProdutoVarianteId, x.ProdutoId })
      .HasPrincipalKey(x => new { x.Id, x.ProdutoId }).OnDelete(DeleteBehavior.Restrict);
  }
}
