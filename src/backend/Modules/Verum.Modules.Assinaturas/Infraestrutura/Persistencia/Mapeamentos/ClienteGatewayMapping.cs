using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Assinaturas.Dominio.ClienteGateway;

namespace Verum.Modules.Assinaturas.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class ClienteGatewayMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("cliente_gateway", table =>
    {
      table.HasCheckConstraint("ck_cliente_gateway_id", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_cliente_gateway_conta_id", "conta_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_cliente_gateway_gateway", "gateway IN (1)");
      table.HasCheckConstraint("ck_cliente_gateway_identificador_externo", "length(btrim(identificador_externo)) > 0");
    });
    builder.HasKey(x => x.Id);
    builder.HasAlternateKey(x => new { x.Id, x.ContaId, x.Gateway });
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
    builder.Property(x => x.ContaId).HasColumnName("conta_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.Gateway).HasColumnName("gateway").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.IdentificadorExterno).HasColumnName("identificador_externo").HasMaxLength(200).HasColumnType("varchar(200)").IsRequired();
    builder.Property(x => x.CriadoEm).HasColumnName("criado_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Property(x => x.AtualizadoEm).HasColumnName("atualizado_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Navigation(x => x.Assinaturas).HasField("_assinaturas").UsePropertyAccessMode(PropertyAccessMode.Field);
    builder.HasIndex(x => new { x.ContaId, x.Gateway }).IsUnique();
    builder.HasIndex(x => new { x.Gateway, x.IdentificadorExterno }).IsUnique();
  }
}
