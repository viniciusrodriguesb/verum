using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Catalogo.Dominio.Categoria;

namespace Verum.Modules.Catalogo.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class CategoriaMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("CATEGORIA", table =>
    {
      table.HasCheckConstraint("CK_CATEGORIA_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_CATEGORIA_CATEGORIA_PAI_ID", "\"CATEGORIA_PAI_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_CATEGORIA_NOME", "length(btrim(\"NOME\")) > 0");

      table.HasCheckConstraint("CK_CATEGORIA_SLUG", "length(btrim(\"SLUG\")) > 0");

      table.HasCheckConstraint("CK_CATEGORIA_REGRA_4", "\"CATEGORIA_PAI_ID\" IS NULL OR \"CATEGORIA_PAI_ID\" <> \"ID\"");
    });

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
           .HasColumnName("ID")
           .HasColumnType("uuid")
           .IsRequired()
           .ValueGeneratedNever();

    builder.Property(x => x.CategoriaPaiId)
           .HasColumnName("CATEGORIA_PAI_ID")
           .HasColumnType("uuid");

    builder.Property(x => x.Nome)
           .HasColumnName("NOME")
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

    builder.Property(x => x.AtualizadaEm)
           .HasColumnName("ATUALIZADA_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.HasOne(x => x.CategoriaPai)
           .WithMany(x => x.Subcategorias)
           .HasForeignKey(x => x.CategoriaPaiId)
           .OnDelete(DeleteBehavior.Restrict);

    builder.Navigation(x => x.Subcategorias)
           .HasField("_subcategorias")
           .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.Navigation(x => x.Produtos)
           .HasField("_produtos")
           .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.HasIndex(x => x.Slug)
           .IsUnique();
  }
}

