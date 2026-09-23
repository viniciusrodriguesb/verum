using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Radar.Dominio.Monitoramento;

namespace Verum.Modules.Radar.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class MonitoramentoMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("MONITORAMENTO", table =>
    {
      table.HasCheckConstraint("CK_MONITORAMENTO_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_MONITORAMENTO_CONTA_ID", "\"CONTA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_MONITORAMENTO_PRODUTO_ID", "\"PRODUTO_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_MONITORAMENTO_PRODUTO_VARIANTE_ID", "\"PRODUTO_VARIANTE_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_MONITORAMENTO_NOME", "length(btrim(\"NOME\")) > 0");

      table.HasCheckConstraint("CK_MONITORAMENTO_PRECO_INICIAL", "\"PRECO_INICIAL\" >= 0.01 AND \"PRECO_INICIAL\" <= 999999999999.99");

      table.HasCheckConstraint("CK_MONITORAMENTO_PRECO_ALVO", "\"PRECO_ALVO\" >= 0.01 AND \"PRECO_ALVO\" <= 999999999999.99");

      table.HasCheckConstraint("CK_MONITORAMENTO_MENOR_PRECO_ATUAL", "\"MENOR_PRECO_ATUAL\" >= 0 AND \"MENOR_PRECO_ATUAL\" <= 999999999999.99");

      table.HasCheckConstraint("CK_MONITORAMENTO_ULTIMA_OFERTA_ID", "\"ULTIMA_OFERTA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_MONITORAMENTO_ULTIMA_OBSERVACAO_ID", "\"ULTIMA_OBSERVACAO_ID\" >= 1 AND \"ULTIMA_OBSERVACAO_ID\" <= 9223372036854775807");

      table.HasCheckConstraint("CK_MONITORAMENTO_STATUS", "\"STATUS\" IN (1, 2, 3)");

      table.HasCheckConstraint("CK_MONITORAMENTO_ULTIMO_PRECO_ALERTADO", "\"ULTIMO_PRECO_ALERTADO\" >= 0 AND \"ULTIMO_PRECO_ALERTADO\" <= 999999999999.99");
    });

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
           .HasColumnName("ID")
           .HasColumnType("uuid")
           .IsRequired()
           .ValueGeneratedNever();

    builder.Property(x => x.ContaId)
           .HasColumnName("CONTA_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.ProdutoId)
           .HasColumnName("PRODUTO_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.ProdutoVarianteId)
           .HasColumnName("PRODUTO_VARIANTE_ID")
           .HasColumnType("uuid");

    builder.Property(x => x.Nome)
           .HasColumnName("NOME")
           .HasMaxLength(250)
           .HasColumnType("varchar(250)")
           .IsRequired();

    builder.Property(x => x.PrecoInicial)
           .HasColumnName("PRECO_INICIAL")
           .HasPrecision(14, 2)
           .HasColumnType("numeric(14,2)")
           .IsRequired();

    builder.Property(x => x.PrecoAlvo)
           .HasColumnName("PRECO_ALVO")
           .HasPrecision(14, 2)
           .HasColumnType("numeric(14,2)")
           .IsRequired();

    builder.Property(x => x.MenorPrecoAtual)
           .HasColumnName("MENOR_PRECO_ATUAL")
           .HasPrecision(14, 2)
           .HasColumnType("numeric(14,2)");

    builder.Property(x => x.UltimaOfertaId)
           .HasColumnName("ULTIMA_OFERTA_ID")
           .HasColumnType("uuid");

    builder.Property(x => x.UltimaObservacaoId)
           .HasColumnName("ULTIMA_OBSERVACAO_ID")
           .HasColumnType("bigint");

    builder.Property(x => x.Status)
           .HasColumnName("STATUS")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.UltimaVerificacaoEm)
           .HasColumnName("ULTIMA_VERIFICACAO_EM")
           .HasColumnType("timestamp with time zone");

    builder.Property(x => x.ProximaVerificacaoEm)
           .HasColumnName("PROXIMA_VERIFICACAO_EM")
           .HasColumnType("timestamp with time zone");

    builder.Property(x => x.CriadoEm)
           .HasColumnName("CRIADO_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Property(x => x.AtualizadoEm)
           .HasColumnName("ATUALIZADO_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Property(x => x.PausadoEm)
           .HasColumnName("PAUSADO_EM")
           .HasColumnType("timestamp with time zone");

    builder.Property(x => x.ExcluidoEm)
           .HasColumnName("EXCLUIDO_EM")
           .HasColumnType("timestamp with time zone");

    builder.Property(x => x.UltimoAlertaEm)
           .HasColumnName("ULTIMO_ALERTA_EM")
           .HasColumnType("timestamp with time zone");

    builder.Property(x => x.UltimoPrecoAlertado)
           .HasColumnName("ULTIMO_PRECO_ALERTADO")
           .HasPrecision(14, 2)
           .HasColumnType("numeric(14,2)");

    builder.Navigation(x => x.Oportunidades)
           .HasField("_oportunidades")
           .UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}

