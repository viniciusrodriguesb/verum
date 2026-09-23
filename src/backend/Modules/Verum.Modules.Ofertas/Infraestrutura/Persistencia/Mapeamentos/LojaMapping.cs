using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Ofertas.Dominio.Loja;

namespace Verum.Modules.Ofertas.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class LojaMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("LOJA", table =>
    {
      table.HasCheckConstraint("CK_LOJA_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_LOJA_NOME", "length(btrim(\"NOME\")) > 0");

      table.HasCheckConstraint("CK_LOJA_NOME_NORMALIZADO", "length(btrim(\"NOME_NORMALIZADO\")) > 0");

      table.HasCheckConstraint("CK_LOJA_DOMINIO", "length(btrim(\"DOMINIO\")) > 0");

      table.HasCheckConstraint("CK_LOJA_URL", "length(btrim(\"URL\")) > 0");

      table.HasCheckConstraint("CK_LOJA_PONTUACAO_CONFIANCA", "\"PONTUACAO_CONFIANCA\" >= 0 AND \"PONTUACAO_CONFIANCA\" <= 100");

      table.HasCheckConstraint("CK_LOJA_STATUS", "\"STATUS\" IN (1, 2, 3)");
    });

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
           .HasColumnName("ID")
           .HasColumnType("uuid")
           .IsRequired()
           .ValueGeneratedNever();

    builder.Property(x => x.Nome)
           .HasColumnName("NOME")
           .HasMaxLength(180)
           .HasColumnType("varchar(180)")
           .IsRequired();

    builder.Property(x => x.NomeNormalizado)
           .HasColumnName("NOME_NORMALIZADO")
           .HasMaxLength(180)
           .HasColumnType("varchar(180)")
           .IsRequired();

    builder.Property(x => x.Dominio)
           .HasColumnName("DOMINIO")
           .HasMaxLength(255)
           .HasColumnType("varchar(255)");

    builder.Property(x => x.Url)
           .HasColumnName("URL")
           .HasColumnType("text");

    builder.Property(x => x.Verificada)
           .HasColumnName("VERIFICADA")
           .HasColumnType("boolean")
           .IsRequired();

    builder.Property(x => x.PontuacaoConfianca)
           .HasColumnName("PONTUACAO_CONFIANCA")
           .HasPrecision(5, 2)
           .HasColumnType("numeric(5,2)")
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

    builder.Navigation(x => x.Ofertas)
           .HasField("_ofertas")
           .UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}

