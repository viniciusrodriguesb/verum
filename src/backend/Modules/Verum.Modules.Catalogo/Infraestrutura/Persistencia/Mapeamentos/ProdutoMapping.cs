using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Catalogo.Dominio.Produto;

namespace Verum.Modules.Catalogo.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class ProdutoMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("PRODUTO", table =>
    {
      table.HasCheckConstraint("CK_PRODUTO_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_PRODUTO_CATEGORIA_ID", "\"CATEGORIA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_PRODUTO_MARCA_ID", "\"MARCA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_PRODUTO_NOME", "length(btrim(\"NOME\")) > 0");

      table.HasCheckConstraint("CK_PRODUTO_NOME_NORMALIZADO", "length(btrim(\"NOME_NORMALIZADO\")) > 0");

      table.HasCheckConstraint("CK_PRODUTO_MODELO", "length(btrim(\"MODELO\")) > 0");

      table.HasCheckConstraint("CK_PRODUTO_SLUG", "length(btrim(\"SLUG\")) > 0");

      table.HasCheckConstraint("CK_PRODUTO_DESCRICAO", "length(btrim(\"DESCRICAO\")) > 0");

      table.HasCheckConstraint("CK_PRODUTO_ATRIBUTOS", "jsonb_typeof(\"ATRIBUTOS\") = 'object'");

      table.HasCheckConstraint("CK_PRODUTO_STATUS", "\"STATUS\" IN (1, 2, 3)");
    });

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
           .HasColumnName("ID")
           .HasColumnType("uuid")
           .IsRequired()
           .ValueGeneratedNever();

    builder.Property(x => x.CategoriaId)
           .HasColumnName("CATEGORIA_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.MarcaId)
           .HasColumnName("MARCA_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.Nome)
           .HasColumnName("NOME")
           .HasMaxLength(250)
           .HasColumnType("varchar(250)")
           .IsRequired();

    builder.Property(x => x.NomeNormalizado)
           .HasColumnName("NOME_NORMALIZADO")
           .HasMaxLength(250)
           .HasColumnType("varchar(250)")
           .IsRequired();

    builder.Property(x => x.Modelo)
           .HasColumnName("MODELO")
           .HasMaxLength(120)
           .HasColumnType("varchar(120)");

    builder.Property(x => x.Slug)
           .HasColumnName("SLUG")
           .HasMaxLength(280)
           .HasColumnType("varchar(280)")
           .IsRequired();

    builder.Property(x => x.Descricao)
           .HasColumnName("DESCRICAO")
           .HasColumnType("text");

    builder.Property(x => x.Atributos)
           .HasColumnName("ATRIBUTOS")
           .HasColumnType("jsonb")
           .IsRequired();

    builder.Property(x => x.Status)
           .HasColumnName("STATUS")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.CriadoEm)
           .HasColumnName("CRIADO_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Property(x => x.AtualizadoEm)
           .HasColumnName("ATUALIZADO_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.HasOne(x => x.Categoria)
           .WithMany(x => x.Produtos)
           .HasForeignKey(x => x.CategoriaId)
           .OnDelete(DeleteBehavior.Restrict);

    builder.HasOne(x => x.Marca)
           .WithMany(x => x.Produtos)
           .HasForeignKey(x => x.MarcaId)
           .OnDelete(DeleteBehavior.Restrict);

    builder.Navigation(x => x.Variantes)
           .HasField("_variantes")
           .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.Navigation(x => x.TermosBusca)
           .HasField("_termosBusca")
           .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.Navigation(x => x.Imagens)
           .HasField("_imagens")
           .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.HasIndex(x => x.Slug)
           .IsUnique();
  }
}

