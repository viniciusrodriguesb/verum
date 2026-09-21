using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Contas.Dominio.Conta;

namespace Verum.Modules.Contas.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class ContaMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("conta", table =>
    {
      table.HasCheckConstraint("ck_conta_id", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_conta_sujeito_identidade", "length(btrim(sujeito_identidade)) > 0");
      table.HasCheckConstraint("ck_conta_email", "length(btrim(email)) > 0");
      table.HasCheckConstraint("ck_conta_nome_exibicao", "length(btrim(nome_exibicao)) > 0");
      table.HasCheckConstraint("ck_conta_status", "status IN (1, 2, 3)");
    });
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
    builder.Property(x => x.SujeitoIdentidade).HasColumnName("sujeito_identidade").HasMaxLength(100).HasColumnType("varchar(100)").IsRequired();
    builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(320).HasColumnType("varchar(320)").IsRequired();
    builder.Property(x => x.NomeExibicao).HasColumnName("nome_exibicao").HasMaxLength(150).HasColumnType("varchar(150)").IsRequired();
    builder.Property(x => x.Status).HasColumnName("status").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.CriadaEm).HasColumnName("criada_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Property(x => x.AtualizadaEm).HasColumnName("atualizada_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Property(x => x.ExcluidaEm).HasColumnName("excluida_em").HasColumnType("timestamp with time zone");
    builder.HasIndex(x => x.SujeitoIdentidade).IsUnique();
  }
}

