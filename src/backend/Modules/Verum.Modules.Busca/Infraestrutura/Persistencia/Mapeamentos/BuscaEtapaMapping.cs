using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Busca.Dominio.BuscaEtapa;

namespace Verum.Modules.Busca.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class BuscaEtapaMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("BUSCA_ETAPA", table =>
    {
      table.HasCheckConstraint("CK_BUSCA_ETAPA_BUSCA_ID", "\"BUSCA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_BUSCA_ETAPA_ETAPA", "\"ETAPA\" IN (1, 2, 3, 4, 5, 6)");

      table.HasCheckConstraint("CK_BUSCA_ETAPA_STATUS", "\"STATUS\" IN (1, 2, 3)");

      table.HasCheckConstraint("CK_BUSCA_ETAPA_DETALHES", "jsonb_typeof(\"DETALHES\") = 'object'");
    });

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
           .HasColumnName("ID")
           .HasColumnType("bigint")
           .IsRequired()
           .UseIdentityByDefaultColumn();

    builder.Property(x => x.BuscaId)
           .HasColumnName("BUSCA_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.Etapa)
           .HasColumnName("ETAPA")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.Status)
           .HasColumnName("STATUS")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.IniciadaEm)
           .HasColumnName("INICIADA_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Property(x => x.ConcluidaEm)
           .HasColumnName("CONCLUIDA_EM")
           .HasColumnType("timestamp with time zone");

    builder.Property(x => x.Detalhes)
           .HasColumnName("DETALHES")
           .HasColumnType("jsonb");

    builder.HasOne(x => x.Busca)
           .WithMany(x => x.Etapas)
           .HasForeignKey(x => x.BuscaId)
           .OnDelete(DeleteBehavior.Restrict);
  }
}

