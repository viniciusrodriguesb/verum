using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Catalogo.Dominio.ProdutoIdentificador;

namespace Verum.Modules.Catalogo.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class ProdutoIdentificadorMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("produto_identificador", table =>
    {
      table.HasCheckConstraint("ck_produto_identificador_id", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_produto_identificador_produto_variante_id", "produto_variante_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_produto_identificador_tipo", "tipo IN (1, 2, 3, 4, 5)");
      table.HasCheckConstraint("ck_produto_identificador_valor", "length(btrim(valor)) > 0");
    });
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
    builder.Property(x => x.ProdutoVarianteId).HasColumnName("produto_variante_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.Tipo).HasColumnName("tipo").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.Valor).HasColumnName("valor").HasMaxLength(100).HasColumnType("varchar(100)").IsRequired();
    builder.Property(x => x.CriadoEm).HasColumnName("criado_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.HasOne(x => x.Variante).WithMany(x => x.Identificadores).HasForeignKey(x => x.ProdutoVarianteId).OnDelete(DeleteBehavior.Restrict);
    builder.HasIndex(x => new { x.Tipo, x.Valor }).IsUnique();
  }
}

