using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Catalogo.Dominio.ProdutoTermoBusca;

namespace Verum.Modules.Catalogo.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class ProdutoTermoBuscaMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("PRODUTO_TERMO_BUSCA", table =>
    {
      table.HasCheckConstraint("CK_PRODUTO_TERMO_BUSCA_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_PRODUTO_TERMO_BUSCA_PRODUTO_ID", "\"PRODUTO_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_PRODUTO_TERMO_BUSCA_PRODUTO_VARIANTE_ID", "\"PRODUTO_VARIANTE_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_PRODUTO_TERMO_BUSCA_TERMO_ORIGINAL", "length(btrim(\"TERMO_ORIGINAL\")) > 0");

      table.HasCheckConstraint("CK_PRODUTO_TERMO_BUSCA_TERMO_NORMALIZADO", "length(btrim(\"TERMO_NORMALIZADO\")) > 0");

      table.HasCheckConstraint("CK_PRODUTO_TERMO_BUSCA_ORIGEM", "\"ORIGEM\" IN (1, 2, 3, 4)");

      table.HasCheckConstraint("CK_PRODUTO_TERMO_BUSCA_CONFIANCA", "\"CONFIANCA\" >= 0 AND \"CONFIANCA\" <= 1");
    });

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
           .HasColumnName("ID")
           .HasColumnType("uuid")
           .IsRequired()
           .ValueGeneratedNever();

    builder.Property(x => x.ProdutoId)
           .HasColumnName("PRODUTO_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.ProdutoVarianteId)
           .HasColumnName("PRODUTO_VARIANTE_ID")
           .HasColumnType("uuid");

    builder.Property(x => x.TermoOriginal)
           .HasColumnName("TERMO_ORIGINAL")
           .HasMaxLength(300)
           .HasColumnType("varchar(300)")
           .IsRequired();

    builder.Property(x => x.TermoNormalizado)
           .HasColumnName("TERMO_NORMALIZADO")
           .HasMaxLength(300)
           .HasColumnType("varchar(300)")
           .IsRequired();

    builder.Property(x => x.Origem)
           .HasColumnName("ORIGEM")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.Confianca)
           .HasColumnName("CONFIANCA")
           .HasPrecision(5, 4)
           .HasColumnType("numeric(5,4)")
           .IsRequired();

    builder.Property(x => x.CriadoEm)
           .HasColumnName("CRIADO_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.HasOne(x => x.Produto)
           .WithMany(x => x.TermosBusca)
           .HasForeignKey(x => x.ProdutoId)
           .OnDelete(DeleteBehavior.Restrict);

    builder.HasOne(x => x.Variante)
           .WithMany(x => x.TermosBusca)
           .HasForeignKey(x => new { x.ProdutoVarianteId, x.ProdutoId })
           .HasPrincipalKey(x => new { x.Id, x.ProdutoId })
           .OnDelete(DeleteBehavior.Restrict);
  }
}
