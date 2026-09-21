using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Busca.Dominio.ResultadoBusca;

namespace Verum.Modules.Busca.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class ResultadoBuscaMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("resultado_busca", table =>
    {
      table.HasCheckConstraint("ck_resultado_busca_id", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_resultado_busca_busca_id", "busca_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_resultado_busca_versao_ranking", "length(btrim(versao_ranking)) > 0");
      table.HasCheckConstraint("ck_resultado_busca_quantidade_analisada", "quantidade_analisada >= 0 AND quantidade_analisada <= 2147483647");
      table.HasCheckConstraint("ck_resultado_busca_quantidade_exibida", "quantidade_exibida >= 0 AND quantidade_exibida <= 2147483647");
      table.HasCheckConstraint("ck_resultado_busca_regra_5", "quantidade_exibida <= quantidade_analisada");
    });
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
    builder.Property(x => x.BuscaId).HasColumnName("busca_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.VersaoRanking).HasColumnName("versao_ranking").HasMaxLength(50).HasColumnType("varchar(50)").IsRequired();
    builder.Property(x => x.QuantidadeAnalisada).HasColumnName("quantidade_analisada").HasColumnType("integer").IsRequired();
    builder.Property(x => x.QuantidadeExibida).HasColumnName("quantidade_exibida").HasColumnType("integer").IsRequired();
    builder.Property(x => x.Parcial).HasColumnName("parcial").HasColumnType("boolean").IsRequired();
    builder.Property(x => x.GeradoEm).HasColumnName("gerado_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.HasOne(x => x.Busca).WithOne(x => x.Resultado).HasForeignKey<Entidade>(x => x.BuscaId).OnDelete(DeleteBehavior.Restrict);
    builder.Navigation(x => x.Ofertas).HasField("_ofertas").UsePropertyAccessMode(PropertyAccessMode.Field);
    builder.HasIndex(x => x.BuscaId).IsUnique();
  }
}

