using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Acesso.Dominio.RegistroUso;

namespace Verum.Modules.Acesso.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class RegistroUsoMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("REGISTRO_USO", table =>
    {
      table.HasCheckConstraint("CK_REGISTRO_USO_CONTA_ID", "\"CONTA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_REGISTRO_USO_VISITANTE_ID", "\"VISITANTE_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_REGISTRO_USO_RECURSO_CODIGO", "length(btrim(\"RECURSO_CODIGO\")) > 0");

      table.HasCheckConstraint("CK_REGISTRO_USO_QUANTIDADE", "\"QUANTIDADE\" >= 1 AND \"QUANTIDADE\" <= 2147483647");

      table.HasCheckConstraint("CK_REGISTRO_USO_REFERENCIA_ID", "\"REFERENCIA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_REGISTRO_USO_DADOS", "jsonb_typeof(\"DADOS\") = 'object'");

      table.HasCheckConstraint("CK_REGISTRO_USO_REGRA_6", "(\"CONTA_ID\" IS NULL) <> (\"VISITANTE_ID\" IS NULL)");
    });

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
           .HasColumnName("ID")
           .HasColumnType("bigint")
           .IsRequired()
           .UseIdentityByDefaultColumn();

    builder.Property(x => x.ContaId)
           .HasColumnName("CONTA_ID")
           .HasColumnType("uuid");

    builder.Property(x => x.VisitanteId)
           .HasColumnName("VISITANTE_ID")
           .HasColumnType("uuid");

    builder.Property(x => x.RecursoCodigo)
           .HasColumnName("RECURSO_CODIGO")
           .HasMaxLength(100)
           .HasColumnType("varchar(100)")
           .IsRequired();

    builder.Property(x => x.Quantidade)
           .HasColumnName("QUANTIDADE")
           .HasColumnType("integer")
           .IsRequired();

    builder.Property(x => x.ReferenciaId)
           .HasColumnName("REFERENCIA_ID")
           .HasColumnType("uuid");

    builder.Property(x => x.Dados)
           .HasColumnName("DADOS")
           .HasColumnType("jsonb");

    builder.Property(x => x.OcorridoEm)
           .HasColumnName("OCORRIDO_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();
  }
}

