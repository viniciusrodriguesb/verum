using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Acesso.Dominio.Recurso;

namespace Verum.Modules.Acesso.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class RecursoMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("RECURSO", table =>
    {
      table.HasCheckConstraint("CK_RECURSO_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_RECURSO_CODIGO", "length(btrim(\"CODIGO\")) > 0");

      table.HasCheckConstraint("CK_RECURSO_NOME", "length(btrim(\"NOME\")) > 0");

      table.HasCheckConstraint("CK_RECURSO_DESCRICAO", "length(btrim(\"DESCRICAO\")) > 0");

      table.HasCheckConstraint("CK_RECURSO_TIPO_LIMITE", "\"TIPO_LIMITE\" IN (1, 2)");
    });

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
           .HasColumnName("ID")
           .HasColumnType("uuid")
           .IsRequired()
           .ValueGeneratedNever();

    builder.Property(x => x.Codigo)
           .HasColumnName("CODIGO")
           .HasMaxLength(100)
           .HasColumnType("varchar(100)")
           .IsRequired();

    builder.Property(x => x.Nome)
           .HasColumnName("NOME")
           .HasMaxLength(150)
           .HasColumnType("varchar(150)")
           .IsRequired();

    builder.Property(x => x.Descricao)
           .HasColumnName("DESCRICAO")
           .HasMaxLength(500)
           .HasColumnType("varchar(500)")
           .IsRequired();

    builder.Property(x => x.TipoLimite)
           .HasColumnName("TIPO_LIMITE")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.Ativo)
           .HasColumnName("ATIVO")
           .HasColumnType("boolean")
           .IsRequired();

    builder.Property(x => x.CriadoEm)
           .HasColumnName("CRIADO_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Navigation(x => x.Pacotes)
           .HasField("_pacotes")
           .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.HasIndex(x => x.Codigo)
           .IsUnique();
  }
}

