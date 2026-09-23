using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Assinaturas.Dominio.ClienteGateway;

namespace Verum.Modules.Assinaturas.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class ClienteGatewayMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("CLIENTE_GATEWAY", table =>
    {
      table.HasCheckConstraint("CK_CLIENTE_GATEWAY_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_CLIENTE_GATEWAY_CONTA_ID", "\"CONTA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_CLIENTE_GATEWAY_GATEWAY", "\"GATEWAY\" IN (1)");

      table.HasCheckConstraint("CK_CLIENTE_GATEWAY_IDENTIFICADOR_EXTERNO", "length(btrim(\"IDENTIFICADOR_EXTERNO\")) > 0");
    });

    builder.HasKey(x => x.Id);

    builder.HasAlternateKey(x => new { x.Id, x.ContaId, x.Gateway });

    builder.Property(x => x.Id)
           .HasColumnName("ID")
           .HasColumnType("uuid")
           .IsRequired()
           .ValueGeneratedNever();

    builder.Property(x => x.ContaId)
           .HasColumnName("CONTA_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.Gateway)
           .HasColumnName("GATEWAY")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.IdentificadorExterno)
           .HasColumnName("IDENTIFICADOR_EXTERNO")
           .HasMaxLength(200)
           .HasColumnType("varchar(200)")
           .IsRequired();

    builder.Property(x => x.CriadoEm)
           .HasColumnName("CRIADO_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Property(x => x.AtualizadoEm)
           .HasColumnName("ATUALIZADO_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Navigation(x => x.Assinaturas)
           .HasField("_assinaturas")
           .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.HasIndex(x => new { x.ContaId, x.Gateway })
           .IsUnique();

    builder.HasIndex(x => new { x.Gateway, x.IdentificadorExterno })
           .IsUnique();
  }
}
