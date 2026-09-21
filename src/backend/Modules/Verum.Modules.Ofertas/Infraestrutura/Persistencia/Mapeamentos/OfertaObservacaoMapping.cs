using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Ofertas.Dominio.OfertaObservacao;

namespace Verum.Modules.Ofertas.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class OfertaObservacaoMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("oferta_observacao", table =>
    {
      table.HasCheckConstraint("ck_oferta_observacao_oferta_id", "oferta_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_oferta_observacao_fonte_id", "fonte_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_oferta_observacao_preco", "preco >= 0.01 AND preco <= 999999999999.99");
      table.HasCheckConstraint("ck_oferta_observacao_preco_pix", "preco_pix >= 0.01 AND preco_pix <= 999999999999.99");
      table.HasCheckConstraint("ck_oferta_observacao_quantidade_parcelas", "quantidade_parcelas >= 1 AND quantidade_parcelas <= 32767");
      table.HasCheckConstraint("ck_oferta_observacao_valor_parcela", "valor_parcela >= 0.01 AND valor_parcela <= 999999999999.99");
      table.HasCheckConstraint("ck_oferta_observacao_condicao_preco", "length(btrim(condicao_preco)) > 0");
      table.HasCheckConstraint("ck_oferta_observacao_disponibilidade", "disponibilidade IN (1, 2, 3)");
      table.HasCheckConstraint("ck_oferta_observacao_hash_conteudo", "length(btrim(hash_conteudo)) > 0");
      table.HasCheckConstraint("ck_oferta_observacao_evidencia", "jsonb_typeof(evidencia) = 'object'");
      table.HasCheckConstraint("ck_oferta_observacao_regra_10", "(quantidade_parcelas IS NULL) = (valor_parcela IS NULL)");
    });
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").IsRequired().UseIdentityByDefaultColumn();
    builder.Property(x => x.OfertaId).HasColumnName("oferta_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.FonteId).HasColumnName("fonte_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.Preco).HasColumnName("preco").HasPrecision(14, 2).HasColumnType("numeric(14,2)").IsRequired();
    builder.Property(x => x.PrecoPix).HasColumnName("preco_pix").HasPrecision(14, 2).HasColumnType("numeric(14,2)");
    builder.Property(x => x.QuantidadeParcelas).HasColumnName("quantidade_parcelas").HasColumnType("smallint");
    builder.Property(x => x.ValorParcela).HasColumnName("valor_parcela").HasPrecision(14, 2).HasColumnType("numeric(14,2)");
    builder.Property(x => x.CondicaoPreco).HasColumnName("condicao_preco").HasMaxLength(300).HasColumnType("varchar(300)");
    builder.Property(x => x.Disponibilidade).HasColumnName("disponibilidade").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.ObservadaEm).HasColumnName("observada_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Property(x => x.HashConteudo).HasColumnName("hash_conteudo").HasMaxLength(64).HasColumnType("varchar(64)").IsRequired();
    builder.Property(x => x.Evidencia).HasColumnName("evidencia").HasColumnType("jsonb");
    builder.HasOne(x => x.Oferta).WithMany(x => x.Observacoes).HasForeignKey(x => x.OfertaId).OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.Fonte).WithMany(x => x.Observacoes).HasForeignKey(x => x.FonteId).OnDelete(DeleteBehavior.Restrict);
  }
}

