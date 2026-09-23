using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Catalogo.Dominio.ProdutoIdentificador;

namespace Verum.Modules.Catalogo.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class ProdutoIdentificadorMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("PRODUTO_IDENTIFICADOR", table =>
    {
      table.HasCheckConstraint("CK_PRODUTO_IDENTIFICADOR_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_PRODUTO_IDENTIFICADOR_PRODUTO_VARIANTE_ID", "\"PRODUTO_VARIANTE_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_PRODUTO_IDENTIFICADOR_TIPO", "\"TIPO\" IN (1, 2, 3, 4, 5)");

      table.HasCheckConstraint("CK_PRODUTO_IDENTIFICADOR_VALOR", "length(btrim(\"VALOR\")) > 0");
    });

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
           .HasColumnName("ID")
           .HasColumnType("uuid")
           .IsRequired()
           .ValueGeneratedNever();

    builder.Property(x => x.ProdutoVarianteId)
           .HasColumnName("PRODUTO_VARIANTE_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.Tipo)
           .HasColumnName("TIPO")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.Valor)
           .HasColumnName("VALOR")
           .HasMaxLength(100)
           .HasColumnType("varchar(100)")
           .IsRequired();

    builder.Property(x => x.CriadoEm)
           .HasColumnName("CRIADO_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.HasOne(x => x.Variante)
           .WithMany(x => x.Identificadores)
           .HasForeignKey(x => x.ProdutoVarianteId)
           .OnDelete(DeleteBehavior.Restrict);

    builder.HasIndex(x => new { x.Tipo, x.Valor })
           .IsUnique();
  }
}

