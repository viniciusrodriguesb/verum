using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Notificacoes.Dominio.EntregaNotificacao;

namespace Verum.Modules.Notificacoes.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class EntregaNotificacaoMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("entrega_notificacao", table =>
    {
      table.HasCheckConstraint("ck_entrega_notificacao_id", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_entrega_notificacao_notificacao_id", "notificacao_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_entrega_notificacao_canal", "canal IN (1, 2, 3)");
      table.HasCheckConstraint("ck_entrega_notificacao_status", "status IN (1, 2, 3, 4, 5, 6)");
      table.HasCheckConstraint("ck_entrega_notificacao_quantidade_tentativas", "quantidade_tentativas >= 0 AND quantidade_tentativas <= 32767");
      table.HasCheckConstraint("ck_entrega_notificacao_codigo_erro", "length(btrim(codigo_erro)) > 0");
      table.HasCheckConstraint("ck_entrega_notificacao_detalhes_erro", "length(btrim(detalhes_erro)) > 0");
      table.HasCheckConstraint("ck_entrega_notificacao_referencia_provedor", "length(btrim(referencia_provedor)) > 0");
    });
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
    builder.Property(x => x.NotificacaoId).HasColumnName("notificacao_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.Canal).HasColumnName("canal").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.Status).HasColumnName("status").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.QuantidadeTentativas).HasColumnName("quantidade_tentativas").HasColumnType("smallint").IsRequired();
    builder.Property(x => x.ProximaTentativaEm).HasColumnName("proxima_tentativa_em").HasColumnType("timestamp with time zone");
    builder.Property(x => x.UltimaTentativaEm).HasColumnName("ultima_tentativa_em").HasColumnType("timestamp with time zone");
    builder.Property(x => x.EntregueEm).HasColumnName("entregue_em").HasColumnType("timestamp with time zone");
    builder.Property(x => x.CodigoErro).HasColumnName("codigo_erro").HasMaxLength(100).HasColumnType("varchar(100)");
    builder.Property(x => x.DetalhesErro).HasColumnName("detalhes_erro").HasMaxLength(1000).HasColumnType("varchar(1000)");
    builder.Property(x => x.ReferenciaProvedor).HasColumnName("referencia_provedor").HasMaxLength(200).HasColumnType("varchar(200)");
    builder.HasOne(x => x.Notificacao).WithMany(x => x.Entregas).HasForeignKey(x => x.NotificacaoId).OnDelete(DeleteBehavior.Restrict);
    builder.HasIndex(x => new { x.NotificacaoId, x.Canal }).IsUnique();
  }
}

