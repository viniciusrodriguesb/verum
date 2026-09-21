using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Radar.Dominio.Oportunidade;

namespace Verum.Modules.Radar.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class OportunidadeMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("oportunidade", table =>
    {
      table.HasCheckConstraint("ck_oportunidade_id", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_oportunidade_monitoramento_id", "monitoramento_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_oportunidade_oferta_id", "oferta_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_oportunidade_oferta_observacao_id", "oferta_observacao_id >= 1 AND oferta_observacao_id <= 9223372036854775807");
      table.HasCheckConstraint("ck_oportunidade_preco_observado", "preco_observado >= 0.01 AND preco_observado <= 999999999999.99");
      table.HasCheckConstraint("ck_oportunidade_preco_alvo", "preco_alvo >= 0.01 AND preco_alvo <= 999999999999.99");
      table.HasCheckConstraint("ck_oportunidade_economia_desde_criacao", "economia_desde_criacao >= -999999999999.99 AND economia_desde_criacao <= 999999999999.99");
      table.HasCheckConstraint("ck_oportunidade_percentual_reducao", "percentual_reducao >= -999 AND percentual_reducao <= 100");
      table.HasCheckConstraint("ck_oportunidade_oferta_snapshot", "jsonb_typeof(oferta_snapshot) = 'object'");
      table.HasCheckConstraint("ck_oportunidade_status", "status IN (1, 2, 3)");
      table.HasCheckConstraint("ck_oportunidade_regra_10", "preco_observado <= preco_alvo");
      table.HasCheckConstraint("ck_oportunidade_regra_11", "expira_em IS NULL OR expira_em > detectada_em");
    });
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
    builder.Property(x => x.MonitoramentoId).HasColumnName("monitoramento_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.OfertaId).HasColumnName("oferta_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.OfertaObservacaoId).HasColumnName("oferta_observacao_id").HasColumnType("bigint").IsRequired();
    builder.Property(x => x.PrecoObservado).HasColumnName("preco_observado").HasPrecision(14, 2).HasColumnType("numeric(14,2)").IsRequired();
    builder.Property(x => x.PrecoAlvo).HasColumnName("preco_alvo").HasPrecision(14, 2).HasColumnType("numeric(14,2)").IsRequired();
    builder.Property(x => x.EconomiaDesdeCriacao).HasColumnName("economia_desde_criacao").HasPrecision(14, 2).HasColumnType("numeric(14,2)");
    builder.Property(x => x.PercentualReducao).HasColumnName("percentual_reducao").HasPrecision(7, 4).HasColumnType("numeric(7,4)");
    builder.Property(x => x.OfertaSnapshot).HasColumnName("oferta_snapshot").HasColumnType("jsonb").IsRequired();
    builder.Property(x => x.Status).HasColumnName("status").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.DetectadaEm).HasColumnName("detectada_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Property(x => x.VisualizadaEm).HasColumnName("visualizada_em").HasColumnType("timestamp with time zone");
    builder.Property(x => x.ExpiraEm).HasColumnName("expira_em").HasColumnType("timestamp with time zone");
    builder.HasOne(x => x.Monitoramento).WithMany(x => x.Oportunidades).HasForeignKey(x => x.MonitoramentoId).OnDelete(DeleteBehavior.Restrict);
    builder.HasIndex(x => new { x.MonitoramentoId, x.OfertaObservacaoId }).IsUnique();
  }
}

