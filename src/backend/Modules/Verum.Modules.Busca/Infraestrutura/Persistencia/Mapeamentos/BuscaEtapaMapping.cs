using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Busca.Dominio.BuscaEtapa;

namespace Verum.Modules.Busca.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class BuscaEtapaMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("busca_etapa", table =>
    {
      table.HasCheckConstraint("ck_busca_etapa_busca_id", "busca_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_busca_etapa_etapa", "etapa IN (1, 2, 3, 4, 5, 6)");
      table.HasCheckConstraint("ck_busca_etapa_status", "status IN (1, 2, 3)");
      table.HasCheckConstraint("ck_busca_etapa_detalhes", "jsonb_typeof(detalhes) = 'object'");
    });
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").IsRequired().UseIdentityByDefaultColumn();
    builder.Property(x => x.BuscaId).HasColumnName("busca_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.Etapa).HasColumnName("etapa").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.Status).HasColumnName("status").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.IniciadaEm).HasColumnName("iniciada_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Property(x => x.ConcluidaEm).HasColumnName("concluida_em").HasColumnType("timestamp with time zone");
    builder.Property(x => x.Detalhes).HasColumnName("detalhes").HasColumnType("jsonb");
    builder.HasOne(x => x.Busca).WithMany(x => x.Etapas).HasForeignKey(x => x.BuscaId).OnDelete(DeleteBehavior.Restrict);
  }
}

