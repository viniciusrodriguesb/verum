using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Acesso.Dominio.ConcessaoPacote;

namespace Verum.Modules.Acesso.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class ConcessaoPacoteMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("concessao_pacote", table =>
    {
      table.HasCheckConstraint("ck_concessao_pacote_id", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_concessao_pacote_conta_id", "conta_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_concessao_pacote_pacote_acesso_id", "pacote_acesso_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_concessao_pacote_origem", "origem IN (1, 2, 3, 4)");
      table.HasCheckConstraint("ck_concessao_pacote_referencia_origem_id", "referencia_origem_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_concessao_pacote_regra_5", "valida_ate IS NULL OR valida_ate > valida_de");
    });
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
    builder.Property(x => x.ContaId).HasColumnName("conta_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.PacoteAcessoId).HasColumnName("pacote_acesso_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.Origem).HasColumnName("origem").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.ReferenciaOrigemId).HasColumnName("referencia_origem_id").HasColumnType("uuid");
    builder.Property(x => x.ValidaDe).HasColumnName("valida_de").HasColumnType("timestamp with time zone").IsRequired();
    builder.Property(x => x.ValidaAte).HasColumnName("valida_ate").HasColumnType("timestamp with time zone");
    builder.Property(x => x.RevogadaEm).HasColumnName("revogada_em").HasColumnType("timestamp with time zone");
    builder.Property(x => x.CriadaEm).HasColumnName("criada_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.HasOne(x => x.Pacote).WithMany(x => x.Concessoes).HasForeignKey(x => x.PacoteAcessoId).OnDelete(DeleteBehavior.Restrict);
  }
}

