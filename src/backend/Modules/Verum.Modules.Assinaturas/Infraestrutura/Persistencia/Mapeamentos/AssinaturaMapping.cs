using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Assinaturas.Dominio.Assinatura;

namespace Verum.Modules.Assinaturas.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class AssinaturaMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("assinatura", table =>
    {
      table.HasCheckConstraint("ck_assinatura_id", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_assinatura_conta_id", "conta_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_assinatura_plano_id", "plano_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_assinatura_cliente_gateway_id", "cliente_gateway_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_assinatura_gateway", "gateway IN (1)");
      table.HasCheckConstraint("ck_assinatura_identificador_externo", "length(btrim(identificador_externo)) > 0");
      table.HasCheckConstraint("ck_assinatura_status", "status IN (1, 2, 3, 4, 5)");
      table.HasCheckConstraint("ck_assinatura_valor_contratado", "valor_contratado >= 0.01 AND valor_contratado <= 999999999999.99");
      table.HasCheckConstraint("ck_assinatura_moeda", "length(btrim(moeda)) > 0");
    });
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
    builder.Property(x => x.ContaId).HasColumnName("conta_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.PlanoId).HasColumnName("plano_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.ClienteGatewayId).HasColumnName("cliente_gateway_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.Gateway).HasColumnName("gateway").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.IdentificadorExterno).HasColumnName("identificador_externo").HasMaxLength(200).HasColumnType("varchar(200)").IsRequired();
    builder.Property(x => x.Status).HasColumnName("status").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.ValorContratado).HasColumnName("valor_contratado").HasPrecision(14, 2).HasColumnType("numeric(14,2)").IsRequired();
    builder.Property(x => x.Moeda).HasColumnName("moeda").HasMaxLength(3).IsFixedLength().HasColumnType("char(3)").IsRequired();
    builder.Property(x => x.PeriodoIniciadoEm).HasColumnName("periodo_iniciado_em").HasColumnType("timestamp with time zone");
    builder.Property(x => x.PeriodoTerminaEm).HasColumnName("periodo_termina_em").HasColumnType("timestamp with time zone");
    builder.Property(x => x.CancelamentoSolicitadoEm).HasColumnName("cancelamento_solicitado_em").HasColumnType("timestamp with time zone");
    builder.Property(x => x.CanceladaEm).HasColumnName("cancelada_em").HasColumnType("timestamp with time zone");
    builder.Property(x => x.CriadaEm).HasColumnName("criada_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Property(x => x.AtualizadaEm).HasColumnName("atualizada_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.HasOne(x => x.Plano).WithMany(x => x.Assinaturas).HasForeignKey(x => x.PlanoId).OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.ClienteGateway).WithMany(x => x.Assinaturas)
      .HasForeignKey(x => new { x.ClienteGatewayId, x.ContaId, x.Gateway })
      .HasPrincipalKey(x => new { x.Id, x.ContaId, x.Gateway }).OnDelete(DeleteBehavior.Restrict);
    builder.Navigation(x => x.Pagamentos).HasField("_pagamentos").UsePropertyAccessMode(PropertyAccessMode.Field);
    builder.HasIndex(x => new { x.Gateway, x.IdentificadorExterno }).IsUnique();
  }
}
