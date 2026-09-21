using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Radar.Dominio.Monitoramento;

namespace Verum.Modules.Radar.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class MonitoramentoMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("monitoramento", table =>
    {
      table.HasCheckConstraint("ck_monitoramento_id", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_monitoramento_conta_id", "conta_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_monitoramento_produto_id", "produto_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_monitoramento_produto_variante_id", "produto_variante_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_monitoramento_nome", "length(btrim(nome)) > 0");
      table.HasCheckConstraint("ck_monitoramento_preco_inicial", "preco_inicial >= 0.01 AND preco_inicial <= 999999999999.99");
      table.HasCheckConstraint("ck_monitoramento_preco_alvo", "preco_alvo >= 0.01 AND preco_alvo <= 999999999999.99");
      table.HasCheckConstraint("ck_monitoramento_menor_preco_atual", "menor_preco_atual >= 0 AND menor_preco_atual <= 999999999999.99");
      table.HasCheckConstraint("ck_monitoramento_ultima_oferta_id", "ultima_oferta_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_monitoramento_ultima_observacao_id", "ultima_observacao_id >= 1 AND ultima_observacao_id <= 9223372036854775807");
      table.HasCheckConstraint("ck_monitoramento_status", "status IN (1, 2, 3)");
      table.HasCheckConstraint("ck_monitoramento_ultimo_preco_alertado", "ultimo_preco_alertado >= 0 AND ultimo_preco_alertado <= 999999999999.99");
    });
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
    builder.Property(x => x.ContaId).HasColumnName("conta_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.ProdutoId).HasColumnName("produto_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.ProdutoVarianteId).HasColumnName("produto_variante_id").HasColumnType("uuid");
    builder.Property(x => x.Nome).HasColumnName("nome").HasMaxLength(250).HasColumnType("varchar(250)").IsRequired();
    builder.Property(x => x.PrecoInicial).HasColumnName("preco_inicial").HasPrecision(14, 2).HasColumnType("numeric(14,2)").IsRequired();
    builder.Property(x => x.PrecoAlvo).HasColumnName("preco_alvo").HasPrecision(14, 2).HasColumnType("numeric(14,2)").IsRequired();
    builder.Property(x => x.MenorPrecoAtual).HasColumnName("menor_preco_atual").HasPrecision(14, 2).HasColumnType("numeric(14,2)");
    builder.Property(x => x.UltimaOfertaId).HasColumnName("ultima_oferta_id").HasColumnType("uuid");
    builder.Property(x => x.UltimaObservacaoId).HasColumnName("ultima_observacao_id").HasColumnType("bigint");
    builder.Property(x => x.Status).HasColumnName("status").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.UltimaVerificacaoEm).HasColumnName("ultima_verificacao_em").HasColumnType("timestamp with time zone");
    builder.Property(x => x.ProximaVerificacaoEm).HasColumnName("proxima_verificacao_em").HasColumnType("timestamp with time zone");
    builder.Property(x => x.CriadoEm).HasColumnName("criado_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Property(x => x.AtualizadoEm).HasColumnName("atualizado_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Property(x => x.PausadoEm).HasColumnName("pausado_em").HasColumnType("timestamp with time zone");
    builder.Property(x => x.ExcluidoEm).HasColumnName("excluido_em").HasColumnType("timestamp with time zone");
    builder.Property(x => x.UltimoAlertaEm).HasColumnName("ultimo_alerta_em").HasColumnType("timestamp with time zone");
    builder.Property(x => x.UltimoPrecoAlertado).HasColumnName("ultimo_preco_alertado").HasPrecision(14, 2).HasColumnType("numeric(14,2)");
    builder.Navigation(x => x.Oportunidades).HasField("_oportunidades").UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}

