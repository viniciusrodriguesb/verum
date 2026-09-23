using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Acesso.Dominio.ConcessaoPacote;

namespace Verum.Modules.Acesso.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class ConcessaoPacoteMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("CONCESSAO_PACOTE", table =>
    {
      table.HasCheckConstraint("CK_CONCESSAO_PACOTE_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_CONCESSAO_PACOTE_CONTA_ID", "\"CONTA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_CONCESSAO_PACOTE_PACOTE_ACESSO_ID", "\"PACOTE_ACESSO_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_CONCESSAO_PACOTE_ORIGEM", "\"ORIGEM\" IN (1, 2, 3, 4)");

      table.HasCheckConstraint("CK_CONCESSAO_PACOTE_REFERENCIA_ORIGEM_ID", "\"REFERENCIA_ORIGEM_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_CONCESSAO_PACOTE_REGRA_5", "\"VALIDA_ATE\" IS NULL OR \"VALIDA_ATE\" > \"VALIDA_DE\"");
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

    builder.Property(x => x.PacoteAcessoId)
           .HasColumnName("PACOTE_ACESSO_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.Origem)
           .HasColumnName("ORIGEM")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.ReferenciaOrigemId)
           .HasColumnName("REFERENCIA_ORIGEM_ID")
           .HasColumnType("uuid");

    builder.Property(x => x.ValidaDe)
           .HasColumnName("VALIDA_DE")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Property(x => x.ValidaAte)
           .HasColumnName("VALIDA_ATE")
           .HasColumnType("timestamp with time zone");

    builder.Property(x => x.RevogadaEm)
           .HasColumnName("REVOGADA_EM")
           .HasColumnType("timestamp with time zone");

    builder.Property(x => x.CriadaEm)
           .HasColumnName("CRIADA_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.HasOne(x => x.Pacote)
           .WithMany(x => x.Concessoes)
           .HasForeignKey(x => x.PacoteAcessoId)
           .OnDelete(DeleteBehavior.Restrict);
  }
}

