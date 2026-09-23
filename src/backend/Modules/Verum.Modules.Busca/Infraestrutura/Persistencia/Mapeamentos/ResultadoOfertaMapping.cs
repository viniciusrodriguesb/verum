using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Busca.Dominio.ResultadoOferta;

namespace Verum.Modules.Busca.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class ResultadoOfertaMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("RESULTADO_OFERTA", table =>
    {
      table.HasCheckConstraint("CK_RESULTADO_OFERTA_RESULTADO_BUSCA_ID", "\"RESULTADO_BUSCA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_RESULTADO_OFERTA_OFERTA_ID", "\"OFERTA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_RESULTADO_OFERTA_PRODUTO_VARIANTE_ID", "\"PRODUTO_VARIANTE_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_RESULTADO_OFERTA_POSICAO", "\"POSICAO\" >= 1 AND \"POSICAO\" <= 32767");

      table.HasCheckConstraint("CK_RESULTADO_OFERTA_CLASSIFICACAO", "\"CLASSIFICACAO\" IN (1, 2, 3)");

      table.HasCheckConstraint("CK_RESULTADO_OFERTA_PONTUACAO_FINAL", "\"PONTUACAO_FINAL\" >= 0 AND \"PONTUACAO_FINAL\" <= 100");

      table.HasCheckConstraint("CK_RESULTADO_OFERTA_PONTUACAO_PRECO", "\"PONTUACAO_PRECO\" >= 0 AND \"PONTUACAO_PRECO\" <= 100");

      table.HasCheckConstraint("CK_RESULTADO_OFERTA_PONTUACAO_CONFIANCA", "\"PONTUACAO_CONFIANCA\" >= 0 AND \"PONTUACAO_CONFIANCA\" <= 100");

      table.HasCheckConstraint("CK_RESULTADO_OFERTA_PONTUACAO_ATUALIZACAO", "\"PONTUACAO_ATUALIZACAO\" >= 0 AND \"PONTUACAO_ATUALIZACAO\" <= 100");

      table.HasCheckConstraint("CK_RESULTADO_OFERTA_EXPLICACAO", "length(btrim(\"EXPLICACAO\")) > 0");

      table.HasCheckConstraint("CK_RESULTADO_OFERTA_OFERTA_SNAPSHOT", "jsonb_typeof(\"OFERTA_SNAPSHOT\") = 'object'");
    });

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
           .HasColumnName("ID")
           .HasColumnType("bigint")
           .IsRequired()
           .UseIdentityByDefaultColumn();

    builder.Property(x => x.ResultadoBuscaId)
           .HasColumnName("RESULTADO_BUSCA_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.OfertaId)
           .HasColumnName("OFERTA_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.ProdutoVarianteId)
           .HasColumnName("PRODUTO_VARIANTE_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.Posicao)
           .HasColumnName("POSICAO")
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.Classificacao)
           .HasColumnName("CLASSIFICACAO")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.PontuacaoFinal)
           .HasColumnName("PONTUACAO_FINAL")
           .HasPrecision(5, 2)
           .HasColumnType("numeric(5,2)")
           .IsRequired();

    builder.Property(x => x.PontuacaoPreco)
           .HasColumnName("PONTUACAO_PRECO")
           .HasPrecision(5, 2)
           .HasColumnType("numeric(5,2)")
           .IsRequired();

    builder.Property(x => x.PontuacaoConfianca)
           .HasColumnName("PONTUACAO_CONFIANCA")
           .HasPrecision(5, 2)
           .HasColumnType("numeric(5,2)")
           .IsRequired();

    builder.Property(x => x.PontuacaoAtualizacao)
           .HasColumnName("PONTUACAO_ATUALIZACAO")
           .HasPrecision(5, 2)
           .HasColumnType("numeric(5,2)")
           .IsRequired();

    builder.Property(x => x.Explicacao)
           .HasColumnName("EXPLICACAO")
           .HasMaxLength(1000)
           .HasColumnType("varchar(1000)")
           .IsRequired();

    builder.Property(x => x.OfertaSnapshot)
           .HasColumnName("OFERTA_SNAPSHOT")
           .HasColumnType("jsonb")
           .IsRequired();

    builder.HasOne(x => x.Resultado)
           .WithMany(x => x.Ofertas)
           .HasForeignKey(x => x.ResultadoBuscaId)
           .OnDelete(DeleteBehavior.Restrict);

    builder.HasIndex(x => new { x.ResultadoBuscaId, x.Posicao })
           .IsUnique();
  }
}

