using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Catalogo.Dominio.ProdutoImagem;

namespace Verum.Modules.Catalogo.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class ProdutoImagemMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("PRODUTO_IMAGEM", table =>
    {
      table.HasCheckConstraint("CK_PRODUTO_IMAGEM_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_PRODUTO_IMAGEM_PRODUTO_ID", "\"PRODUTO_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_PRODUTO_IMAGEM_PRODUTO_VARIANTE_ID", "\"PRODUTO_VARIANTE_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_PRODUTO_IMAGEM_URL", "length(btrim(\"URL\")) > 0");

      table.HasCheckConstraint("CK_PRODUTO_IMAGEM_ORIGEM", "length(btrim(\"ORIGEM\")) > 0");

      table.HasCheckConstraint("CK_PRODUTO_IMAGEM_ORDEM", "\"ORDEM\" >= 0 AND \"ORDEM\" <= 32767");
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

    builder.Property(x => x.Url)
           .HasColumnName("URL")
           .HasColumnType("text")
           .IsRequired();

    builder.Property(x => x.Origem)
           .HasColumnName("ORIGEM")
           .HasMaxLength(100)
           .HasColumnType("varchar(100)")
           .IsRequired();

    builder.Property(x => x.Principal)
           .HasColumnName("PRINCIPAL")
           .HasColumnType("boolean")
           .IsRequired();

    builder.Property(x => x.Ordem)
           .HasColumnName("ORDEM")
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.CriadaEm)
           .HasColumnName("CRIADA_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.HasOne(x => x.Produto)
           .WithMany(x => x.Imagens)
           .HasForeignKey(x => x.ProdutoId)
           .OnDelete(DeleteBehavior.Restrict);

    builder.HasOne(x => x.Variante)
           .WithMany(x => x.Imagens)
           .HasForeignKey(x => new { x.ProdutoVarianteId, x.ProdutoId })
           .HasPrincipalKey(x => new { x.Id, x.ProdutoId })
           .OnDelete(DeleteBehavior.Restrict);
  }
}
