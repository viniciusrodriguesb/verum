using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Acesso.Dominio.PacoteRecurso;

namespace Verum.Modules.Acesso.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class PacoteRecursoMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("pacote_recurso", table =>
    {
      table.HasCheckConstraint("ck_pacote_recurso_id", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_pacote_recurso_pacote_acesso_id", "pacote_acesso_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_pacote_recurso_recurso_id", "recurso_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_pacote_recurso_limite_quantidade", "limite_quantidade >= 0 AND limite_quantidade <= 2147483647");
      table.HasCheckConstraint("ck_pacote_recurso_periodicidade", "periodicidade IN (1, 2, 3)");
      table.HasCheckConstraint("ck_pacote_recurso_configuracao", "jsonb_typeof(configuracao) = 'object'");
    });
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
    builder.Property(x => x.PacoteAcessoId).HasColumnName("pacote_acesso_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.RecursoId).HasColumnName("recurso_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.Habilitado).HasColumnName("habilitado").HasColumnType("boolean").IsRequired();
    builder.Property(x => x.LimiteQuantidade).HasColumnName("limite_quantidade").HasColumnType("integer");
    builder.Property(x => x.Periodicidade).HasColumnName("periodicidade").HasConversion<short>().HasColumnType("smallint");
    builder.Property(x => x.Configuracao).HasColumnName("configuracao").HasColumnType("jsonb");
    builder.HasOne(x => x.Pacote).WithMany(x => x.Recursos).HasForeignKey(x => x.PacoteAcessoId).OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.Recurso).WithMany(x => x.Pacotes).HasForeignKey(x => x.RecursoId).OnDelete(DeleteBehavior.Restrict);
    builder.HasIndex(x => new { x.PacoteAcessoId, x.RecursoId }).IsUnique();
  }
}

