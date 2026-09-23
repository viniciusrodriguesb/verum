using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Contas.Dominio.Conta;

namespace Verum.Modules.Contas.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class ContaMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("CONTA", table =>
    {
      table.HasCheckConstraint("CK_CONTA_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_CONTA_SUJEITO_IDENTIDADE", "length(btrim(\"SUJEITO_IDENTIDADE\")) > 0");

      table.HasCheckConstraint("CK_CONTA_EMAIL", "length(btrim(\"EMAIL\")) > 0");

      table.HasCheckConstraint("CK_CONTA_NOME_EXIBICAO", "length(btrim(\"NOME_EXIBICAO\")) > 0");

      table.HasCheckConstraint("CK_CONTA_STATUS", "\"STATUS\" IN (1, 2, 3)");
    });

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
           .HasColumnName("ID")
           .HasColumnType("uuid")
           .IsRequired()
           .ValueGeneratedNever();

    builder.Property(x => x.SujeitoIdentidade)
           .HasColumnName("SUJEITO_IDENTIDADE")
           .HasMaxLength(100)
           .HasColumnType("varchar(100)")
           .IsRequired();

    builder.Property(x => x.Email)
           .HasColumnName("EMAIL")
           .HasMaxLength(320)
           .HasColumnType("varchar(320)")
           .IsRequired();

    builder.Property(x => x.NomeExibicao)
           .HasColumnName("NOME_EXIBICAO")
           .HasMaxLength(150)
           .HasColumnType("varchar(150)")
           .IsRequired();

    builder.Property(x => x.Status)
           .HasColumnName("STATUS")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.CriadaEm)
           .HasColumnName("CRIADA_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Property(x => x.AtualizadaEm)
           .HasColumnName("ATUALIZADA_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Property(x => x.ExcluidaEm)
           .HasColumnName("EXCLUIDA_EM")
           .HasColumnType("timestamp with time zone");

    builder.HasIndex(x => x.SujeitoIdentidade)
           .IsUnique();
  }
}

