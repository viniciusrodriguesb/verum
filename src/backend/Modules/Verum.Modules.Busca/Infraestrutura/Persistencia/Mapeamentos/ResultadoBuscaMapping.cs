using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Busca.Dominio.ResultadoBusca;

namespace Verum.Modules.Busca.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class ResultadoBuscaMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("RESULTADO_BUSCA", table =>
    {
      table.HasCheckConstraint("CK_RESULTADO_BUSCA_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_RESULTADO_BUSCA_BUSCA_ID", "\"BUSCA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_RESULTADO_BUSCA_VERSAO_RANKING", "length(btrim(\"VERSAO_RANKING\")) > 0");

      table.HasCheckConstraint("CK_RESULTADO_BUSCA_QUANTIDADE_ANALISADA", "\"QUANTIDADE_ANALISADA\" >= 0 AND \"QUANTIDADE_ANALISADA\" <= 2147483647");

      table.HasCheckConstraint("CK_RESULTADO_BUSCA_QUANTIDADE_EXIBIDA", "\"QUANTIDADE_EXIBIDA\" >= 0 AND \"QUANTIDADE_EXIBIDA\" <= 2147483647");

      table.HasCheckConstraint("CK_RESULTADO_BUSCA_REGRA_5", "\"QUANTIDADE_EXIBIDA\" <= \"QUANTIDADE_ANALISADA\"");
    });

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
           .HasColumnName("ID")
           .HasColumnType("uuid")
           .IsRequired()
           .ValueGeneratedNever();

    builder.Property(x => x.BuscaId)
           .HasColumnName("BUSCA_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.VersaoRanking)
           .HasColumnName("VERSAO_RANKING")
           .HasMaxLength(50)
           .HasColumnType("varchar(50)")
           .IsRequired();

    builder.Property(x => x.QuantidadeAnalisada)
           .HasColumnName("QUANTIDADE_ANALISADA")
           .HasColumnType("integer")
           .IsRequired();

    builder.Property(x => x.QuantidadeExibida)
           .HasColumnName("QUANTIDADE_EXIBIDA")
           .HasColumnType("integer")
           .IsRequired();

    builder.Property(x => x.Parcial)
           .HasColumnName("PARCIAL")
           .HasColumnType("boolean")
           .IsRequired();

    builder.Property(x => x.GeradoEm)
           .HasColumnName("GERADO_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.HasOne(x => x.Busca)
           .WithOne(x => x.Resultado)
           .HasForeignKey<Entidade>(x => x.BuscaId)
           .OnDelete(DeleteBehavior.Restrict);

    builder.Navigation(x => x.Ofertas)
           .HasField("_ofertas")
           .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.HasIndex(x => x.BuscaId)
           .IsUnique();
  }
}

