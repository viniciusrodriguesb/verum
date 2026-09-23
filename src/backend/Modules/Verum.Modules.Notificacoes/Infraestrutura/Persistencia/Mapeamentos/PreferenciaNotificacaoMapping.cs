using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Notificacoes.Dominio.PreferenciaNotificacao;

namespace Verum.Modules.Notificacoes.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class PreferenciaNotificacaoMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("PREFERENCIA_NOTIFICACAO", table =>
    {
      table.HasCheckConstraint("CK_PREFERENCIA_NOTIFICACAO_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_PREFERENCIA_NOTIFICACAO_CONTA_ID", "\"CONTA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_PREFERENCIA_NOTIFICACAO_CANAL", "\"CANAL\" IN (1, 2, 3)");
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

    builder.Property(x => x.Canal)
           .HasColumnName("CANAL")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.Habilitada)
           .HasColumnName("HABILITADA")
           .HasColumnType("boolean")
           .IsRequired();

    builder.Property(x => x.CriadaEm)
           .HasColumnName("CRIADA_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Property(x => x.AtualizadaEm)
           .HasColumnName("ATUALIZADA_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.HasIndex(x => new { x.ContaId, x.Canal })
           .IsUnique();
  }
}

