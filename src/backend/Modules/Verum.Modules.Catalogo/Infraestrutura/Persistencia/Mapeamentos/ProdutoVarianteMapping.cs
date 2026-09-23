using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Catalogo.Dominio.ProdutoVariante;

namespace Verum.Modules.Catalogo.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class ProdutoVarianteMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("PRODUTO_VARIANTE", table =>
    {
      table.HasCheckConstraint("CK_PRODUTO_VARIANTE_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_PRODUTO_VARIANTE_PRODUTO_ID", "\"PRODUTO_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_PRODUTO_VARIANTE_NOME", "length(btrim(\"NOME\")) > 0");

      table.HasCheckConstraint("CK_PRODUTO_VARIANTE_NOME_NORMALIZADO", "length(btrim(\"NOME_NORMALIZADO\")) > 0");

      table.HasCheckConstraint("CK_PRODUTO_VARIANTE_SLUG", "length(btrim(\"SLUG\")) > 0");

      table.HasCheckConstraint("CK_PRODUTO_VARIANTE_ATRIBUTOS", "jsonb_typeof(\"ATRIBUTOS\") = 'object'");

      table.HasCheckConstraint("CK_PRODUTO_VARIANTE_STATUS", "\"STATUS\" IN (1, 2, 3)");
    });

    builder.HasKey(x => x.Id);

    builder.HasAlternateKey(x => new { x.Id, x.ProdutoId });

    builder.Property(x => x.Id)
           .HasColumnName("ID")
           .HasColumnType("uuid")
           .IsRequired()
           .ValueGeneratedNever();

    builder.Property(x => x.ProdutoId)
           .HasColumnName("PRODUTO_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.Nome)
           .HasColumnName("NOME")
           .HasMaxLength(300)
           .HasColumnType("varchar(300)")
           .IsRequired();

    builder.Property(x => x.NomeNormalizado)
           .HasColumnName("NOME_NORMALIZADO")
           .HasMaxLength(300)
           .HasColumnType("varchar(300)")
           .IsRequired();

    builder.Property(x => x.Slug)
           .HasColumnName("SLUG")
           .HasMaxLength(320)
           .HasColumnType("varchar(320)")
           .IsRequired();

    builder.Property(x => x.Atributos)
           .HasColumnName("ATRIBUTOS")
           .HasColumnType("jsonb")
           .IsRequired();

    builder.Property(x => x.Status)
           .HasColumnName("STATUS")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.CriadaEm)
           .HasColumnName("CRIADA_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Property(x => x.AtualizadaEm)
           .HasColumnName("ATUALIZADA_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.HasOne(x => x.Produto)
           .WithMany(x => x.Variantes)
           .HasForeignKey(x => x.ProdutoId)
           .OnDelete(DeleteBehavior.Restrict);

    builder.Navigation(x => x.Identificadores)
           .HasField("_identificadores")
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
