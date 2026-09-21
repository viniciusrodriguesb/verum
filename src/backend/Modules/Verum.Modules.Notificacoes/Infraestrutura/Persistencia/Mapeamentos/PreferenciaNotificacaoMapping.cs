using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Notificacoes.Dominio.PreferenciaNotificacao;

namespace Verum.Modules.Notificacoes.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class PreferenciaNotificacaoMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("preferencia_notificacao", table =>
    {
      table.HasCheckConstraint("ck_preferencia_notificacao_id", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_preferencia_notificacao_conta_id", "conta_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_preferencia_notificacao_canal", "canal IN (1, 2, 3)");
    });
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
    builder.Property(x => x.ContaId).HasColumnName("conta_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.Canal).HasColumnName("canal").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.Habilitada).HasColumnName("habilitada").HasColumnType("boolean").IsRequired();
    builder.Property(x => x.CriadaEm).HasColumnName("criada_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Property(x => x.AtualizadaEm).HasColumnName("atualizada_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.HasIndex(x => new { x.ContaId, x.Canal }).IsUnique();
  }
}

