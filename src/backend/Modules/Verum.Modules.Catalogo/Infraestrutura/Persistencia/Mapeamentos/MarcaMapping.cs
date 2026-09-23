using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Catalogo.Dominio.Marca;

namespace Verum.Modules.Catalogo.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class MarcaMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("MARCA", table =>
    {
      table.HasCheckConstraint("CK_MARCA_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_MARCA_NOME", "length(btrim(\"NOME\")) > 0");

      table.HasCheckConstraint("CK_MARCA_NOME_NORMALIZADO", "length(btrim(\"NOME_NORMALIZADO\")) > 0");

      table.HasCheckConstraint("CK_MARCA_SLUG", "length(btrim(\"SLUG\")) > 0");
    });

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
           .HasColumnName("ID")
           .HasColumnType("uuid")
           .IsRequired()
           .ValueGeneratedNever();

    builder.Property(x => x.Nome)
           .HasColumnName("NOME")
           .HasMaxLength(120)
           .HasColumnType("varchar(120)")
           .IsRequired();

    builder.Property(x => x.NomeNormalizado)
           .HasColumnName("NOME_NORMALIZADO")
           .HasMaxLength(120)
           .HasColumnType("varchar(120)")
           .IsRequired();

    builder.Property(x => x.Slug)
           .HasColumnName("SLUG")
           .HasMaxLength(140)
           .HasColumnType("varchar(140)")
           .IsRequired();

    builder.Property(x => x.Ativa)
           .HasColumnName("ATIVA")
           .HasColumnType("boolean")
           .IsRequired();

    builder.Property(x => x.CriadaEm)
           .HasColumnName("CRIADA_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Navigation(x => x.Produtos)
           .HasField("_produtos")
           .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.HasIndex(x => x.Slug)
           .IsUnique();

    builder.HasIndex(x => x.NomeNormalizado)
           .IsUnique();
  }
}

