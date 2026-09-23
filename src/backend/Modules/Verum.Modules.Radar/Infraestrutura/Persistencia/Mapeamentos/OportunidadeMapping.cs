using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Radar.Dominio.Oportunidade;

namespace Verum.Modules.Radar.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class OportunidadeMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("OPORTUNIDADE", table =>
    {
      table.HasCheckConstraint("CK_OPORTUNIDADE_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_OPORTUNIDADE_MONITORAMENTO_ID", "\"MONITORAMENTO_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_OPORTUNIDADE_OFERTA_ID", "\"OFERTA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_OPORTUNIDADE_OFERTA_OBSERVACAO_ID", "\"OFERTA_OBSERVACAO_ID\" >= 1 AND \"OFERTA_OBSERVACAO_ID\" <= 9223372036854775807");

      table.HasCheckConstraint("CK_OPORTUNIDADE_PRECO_OBSERVADO", "\"PRECO_OBSERVADO\" >= 0.01 AND \"PRECO_OBSERVADO\" <= 999999999999.99");

      table.HasCheckConstraint("CK_OPORTUNIDADE_PRECO_ALVO", "\"PRECO_ALVO\" >= 0.01 AND \"PRECO_ALVO\" <= 999999999999.99");

      table.HasCheckConstraint("CK_OPORTUNIDADE_ECONOMIA_DESDE_CRIACAO", "\"ECONOMIA_DESDE_CRIACAO\" >= -999999999999.99 AND \"ECONOMIA_DESDE_CRIACAO\" <= 999999999999.99");

      table.HasCheckConstraint("CK_OPORTUNIDADE_PERCENTUAL_REDUCAO", "\"PERCENTUAL_REDUCAO\" >= -999 AND \"PERCENTUAL_REDUCAO\" <= 100");

      table.HasCheckConstraint("CK_OPORTUNIDADE_OFERTA_SNAPSHOT", "jsonb_typeof(\"OFERTA_SNAPSHOT\") = 'object'");

      table.HasCheckConstraint("CK_OPORTUNIDADE_STATUS", "\"STATUS\" IN (1, 2, 3)");

      table.HasCheckConstraint("CK_OPORTUNIDADE_REGRA_10", "\"PRECO_OBSERVADO\" <= \"PRECO_ALVO\"");

      table.HasCheckConstraint("CK_OPORTUNIDADE_REGRA_11", "\"EXPIRA_EM\" IS NULL OR \"EXPIRA_EM\" > \"DETECTADA_EM\"");
    });

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
           .HasColumnName("ID")
           .HasColumnType("uuid")
           .IsRequired()
           .ValueGeneratedNever();

    builder.Property(x => x.MonitoramentoId)
           .HasColumnName("MONITORAMENTO_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.OfertaId)
           .HasColumnName("OFERTA_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.OfertaObservacaoId)
           .HasColumnName("OFERTA_OBSERVACAO_ID")
           .HasColumnType("bigint")
           .IsRequired();

    builder.Property(x => x.PrecoObservado)
           .HasColumnName("PRECO_OBSERVADO")
           .HasPrecision(14, 2)
           .HasColumnType("numeric(14,2)")
           .IsRequired();

    builder.Property(x => x.PrecoAlvo)
           .HasColumnName("PRECO_ALVO")
           .HasPrecision(14, 2)
           .HasColumnType("numeric(14,2)")
           .IsRequired();

    builder.Property(x => x.EconomiaDesdeCriacao)
           .HasColumnName("ECONOMIA_DESDE_CRIACAO")
           .HasPrecision(14, 2)
           .HasColumnType("numeric(14,2)");

    builder.Property(x => x.PercentualReducao)
           .HasColumnName("PERCENTUAL_REDUCAO")
           .HasPrecision(7, 4)
           .HasColumnType("numeric(7,4)");

    builder.Property(x => x.OfertaSnapshot)
           .HasColumnName("OFERTA_SNAPSHOT")
           .HasColumnType("jsonb")
           .IsRequired();

    builder.Property(x => x.Status)
           .HasColumnName("STATUS")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.DetectadaEm)
           .HasColumnName("DETECTADA_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Property(x => x.VisualizadaEm)
           .HasColumnName("VISUALIZADA_EM")
           .HasColumnType("timestamp with time zone");

    builder.Property(x => x.ExpiraEm)
           .HasColumnName("EXPIRA_EM")
           .HasColumnType("timestamp with time zone");

    builder.HasOne(x => x.Monitoramento)
           .WithMany(x => x.Oportunidades)
           .HasForeignKey(x => x.MonitoramentoId)
           .OnDelete(DeleteBehavior.Restrict);

    builder.HasIndex(x => new { x.MonitoramentoId, x.OfertaObservacaoId })
           .IsUnique();
  }
}

