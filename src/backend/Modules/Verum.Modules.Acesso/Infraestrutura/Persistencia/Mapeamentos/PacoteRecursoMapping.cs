using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Acesso.Dominio.PacoteRecurso;

namespace Verum.Modules.Acesso.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class PacoteRecursoMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("PACOTE_RECURSO", table =>
    {
      table.HasCheckConstraint("CK_PACOTE_RECURSO_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_PACOTE_RECURSO_PACOTE_ACESSO_ID", "\"PACOTE_ACESSO_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_PACOTE_RECURSO_RECURSO_ID", "\"RECURSO_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_PACOTE_RECURSO_LIMITE_QUANTIDADE", "\"LIMITE_QUANTIDADE\" >= 0 AND \"LIMITE_QUANTIDADE\" <= 2147483647");

      table.HasCheckConstraint("CK_PACOTE_RECURSO_PERIODICIDADE", "\"PERIODICIDADE\" IN (1, 2, 3)");

      table.HasCheckConstraint("CK_PACOTE_RECURSO_CONFIGURACAO", "jsonb_typeof(\"CONFIGURACAO\") = 'object'");
    });

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
           .HasColumnName("ID")
           .HasColumnType("uuid")
           .IsRequired()
           .ValueGeneratedNever();

    builder.Property(x => x.PacoteAcessoId)
           .HasColumnName("PACOTE_ACESSO_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.RecursoId)
           .HasColumnName("RECURSO_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.Habilitado)
           .HasColumnName("HABILITADO")
           .HasColumnType("boolean")
           .IsRequired();

    builder.Property(x => x.LimiteQuantidade)
           .HasColumnName("LIMITE_QUANTIDADE")
           .HasColumnType("integer");

    builder.Property(x => x.Periodicidade)
           .HasColumnName("PERIODICIDADE")
           .HasConversion<short>()
           .HasColumnType("smallint");

    builder.Property(x => x.Configuracao)
           .HasColumnName("CONFIGURACAO")
           .HasColumnType("jsonb");

    builder.HasOne(x => x.Pacote)
           .WithMany(x => x.Recursos)
           .HasForeignKey(x => x.PacoteAcessoId)
           .OnDelete(DeleteBehavior.Restrict);

    builder.HasOne(x => x.Recurso)
           .WithMany(x => x.Pacotes)
           .HasForeignKey(x => x.RecursoId)
           .OnDelete(DeleteBehavior.Restrict);

    builder.HasIndex(x => new { x.PacoteAcessoId, x.RecursoId })
           .IsUnique();
  }
}

