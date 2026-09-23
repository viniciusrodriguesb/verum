using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Ofertas.Dominio.FonteOferta;

namespace Verum.Modules.Ofertas.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class FonteOfertaMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("FONTE_OFERTA", table =>
    {
      table.HasCheckConstraint("CK_FONTE_OFERTA_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_FONTE_OFERTA_NOME", "length(btrim(\"NOME\")) > 0");

      table.HasCheckConstraint("CK_FONTE_OFERTA_TIPO", "\"TIPO\" IN (1, 2, 3, 4, 5)");

      table.HasCheckConstraint("CK_FONTE_OFERTA_CODIGO", "length(btrim(\"CODIGO\")) > 0");

      table.HasCheckConstraint("CK_FONTE_OFERTA_NIVEL_CONFIANCA", "\"NIVEL_CONFIANCA\" >= 0 AND \"NIVEL_CONFIANCA\" <= 100");
    });

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
           .HasColumnName("ID")
           .HasColumnType("uuid")
           .IsRequired()
           .ValueGeneratedNever();

    builder.Property(x => x.Nome)
           .HasColumnName("NOME")
           .HasMaxLength(120)
           .HasColumnType("varchar(120)")
           .IsRequired();

    builder.Property(x => x.Tipo)
           .HasColumnName("TIPO")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.Codigo)
           .HasColumnName("CODIGO")
           .HasMaxLength(80)
           .HasColumnType("varchar(80)")
           .IsRequired();

    builder.Property(x => x.Ativa)
           .HasColumnName("ATIVA")
           .HasColumnType("boolean")
           .IsRequired();

    builder.Property(x => x.NivelConfianca)
           .HasColumnName("NIVEL_CONFIANCA")
           .HasPrecision(5, 2)
           .HasColumnType("numeric(5,2)")
           .IsRequired();

    builder.Property(x => x.CriadaEm)
           .HasColumnName("CRIADA_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Navigation(x => x.Ofertas)
           .HasField("_ofertas")
           .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.Navigation(x => x.Observacoes)
           .HasField("_observacoes")
           .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.Navigation(x => x.Execucoes)
           .HasField("_execucoes")
           .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.HasIndex(x => x.Codigo)
           .IsUnique();
  }
}

