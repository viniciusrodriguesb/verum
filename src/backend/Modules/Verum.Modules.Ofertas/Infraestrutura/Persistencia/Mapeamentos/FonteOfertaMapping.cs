using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Ofertas.Dominio.FonteOferta;

namespace Verum.Modules.Ofertas.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class FonteOfertaMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("fonte_oferta", table =>
    {
      table.HasCheckConstraint("ck_fonte_oferta_id", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_fonte_oferta_nome", "length(btrim(nome)) > 0");
      table.HasCheckConstraint("ck_fonte_oferta_tipo", "tipo IN (1, 2, 3, 4, 5)");
      table.HasCheckConstraint("ck_fonte_oferta_codigo", "length(btrim(codigo)) > 0");
      table.HasCheckConstraint("ck_fonte_oferta_nivel_confianca", "nivel_confianca >= 0 AND nivel_confianca <= 100");
    });

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
           .HasColumnName("id")
           .HasColumnType("uuid")
           .IsRequired()
           .ValueGeneratedNever();

    builder.Property(x => x.Nome)
           .HasColumnName("nome")
           .HasMaxLength(120)
           .HasColumnType("varchar(120)")
           .IsRequired();

    builder.Property(x => x.Tipo)
           .HasColumnName("tipo")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.Codigo)
           .HasColumnName("codigo")
           .HasMaxLength(80)
           .HasColumnType("varchar(80)")
           .IsRequired();

    builder.Property(x => x.Ativa)
           .HasColumnName("ativa")
           .HasColumnType("boolean")
           .IsRequired();

    builder.Property(x => x.NivelConfianca)
           .HasColumnName("nivel_confianca")
           .HasPrecision(5, 2)
           .HasColumnType("numeric(5,2)")
           .IsRequired();

    builder.Property(x => x.CriadaEm)
           .HasColumnName("criada_em")
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

    builder.HasIndex(x => x.Codigo).IsUnique();
  }
}

