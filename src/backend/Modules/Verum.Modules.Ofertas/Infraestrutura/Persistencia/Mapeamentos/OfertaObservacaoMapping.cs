using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Ofertas.Dominio.OfertaObservacao;

namespace Verum.Modules.Ofertas.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class OfertaObservacaoMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("OFERTA_OBSERVACAO", table =>
    {
      table.HasCheckConstraint("CK_OFERTA_OBSERVACAO_OFERTA_ID", "\"OFERTA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_OFERTA_OBSERVACAO_FONTE_ID", "\"FONTE_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_OFERTA_OBSERVACAO_PRECO", "\"PRECO\" >= 0.01 AND \"PRECO\" <= 999999999999.99");

      table.HasCheckConstraint("CK_OFERTA_OBSERVACAO_PRECO_PIX", "\"PRECO_PIX\" >= 0.01 AND \"PRECO_PIX\" <= 999999999999.99");

      table.HasCheckConstraint("CK_OFERTA_OBSERVACAO_QUANTIDADE_PARCELAS", "\"QUANTIDADE_PARCELAS\" >= 1 AND \"QUANTIDADE_PARCELAS\" <= 32767");

      table.HasCheckConstraint("CK_OFERTA_OBSERVACAO_VALOR_PARCELA", "\"VALOR_PARCELA\" >= 0.01 AND \"VALOR_PARCELA\" <= 999999999999.99");

      table.HasCheckConstraint("CK_OFERTA_OBSERVACAO_CONDICAO_PRECO", "length(btrim(\"CONDICAO_PRECO\")) > 0");

      table.HasCheckConstraint("CK_OFERTA_OBSERVACAO_DISPONIBILIDADE", "\"DISPONIBILIDADE\" IN (1, 2, 3)");

      table.HasCheckConstraint("CK_OFERTA_OBSERVACAO_HASH_CONTEUDO", "length(btrim(\"HASH_CONTEUDO\")) > 0");

      table.HasCheckConstraint("CK_OFERTA_OBSERVACAO_EVIDENCIA", "jsonb_typeof(\"EVIDENCIA\") = 'object'");

      table.HasCheckConstraint("CK_OFERTA_OBSERVACAO_REGRA_10", "(\"QUANTIDADE_PARCELAS\" IS NULL) = (\"VALOR_PARCELA\" IS NULL)");
    });

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
           .HasColumnName("ID")
           .HasColumnType("bigint")
           .IsRequired()
           .UseIdentityByDefaultColumn();

    builder.Property(x => x.OfertaId)
           .HasColumnName("OFERTA_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.FonteId)
           .HasColumnName("FONTE_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.Preco)
           .HasColumnName("PRECO")
           .HasPrecision(14, 2)
           .HasColumnType("numeric(14,2)")
           .IsRequired();

    builder.Property(x => x.PrecoPix)
           .HasColumnName("PRECO_PIX")
           .HasPrecision(14, 2)
           .HasColumnType("numeric(14,2)");

    builder.Property(x => x.QuantidadeParcelas)
           .HasColumnName("QUANTIDADE_PARCELAS")
           .HasColumnType("smallint");

    builder.Property(x => x.ValorParcela)
           .HasColumnName("VALOR_PARCELA")
           .HasPrecision(14, 2)
           .HasColumnType("numeric(14,2)");

    builder.Property(x => x.CondicaoPreco)
           .HasColumnName("CONDICAO_PRECO")
           .HasMaxLength(300)
           .HasColumnType("varchar(300)");

    builder.Property(x => x.Disponibilidade)
           .HasColumnName("DISPONIBILIDADE")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.ObservadaEm)
           .HasColumnName("OBSERVADA_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Property(x => x.HashConteudo)
           .HasColumnName("HASH_CONTEUDO")
           .HasMaxLength(64)
           .HasColumnType("varchar(64)")
           .IsRequired();

    builder.Property(x => x.Evidencia)
           .HasColumnName("EVIDENCIA")
           .HasColumnType("jsonb");

    builder.HasOne(x => x.Oferta)
           .WithMany(x => x.Observacoes)
           .HasForeignKey(x => x.OfertaId)
           .OnDelete(DeleteBehavior.Restrict);

    builder.HasOne(x => x.Fonte)
           .WithMany(x => x.Observacoes)
           .HasForeignKey(x => x.FonteId)
           .OnDelete(DeleteBehavior.Restrict);
  }
}

