using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Assinaturas.Dominio.Assinatura;

namespace Verum.Modules.Assinaturas.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class AssinaturaMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("ASSINATURA", table =>
    {
      table.HasCheckConstraint("CK_ASSINATURA_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_ASSINATURA_CONTA_ID", "\"CONTA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_ASSINATURA_PLANO_ID", "\"PLANO_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_ASSINATURA_CLIENTE_GATEWAY_ID", "\"CLIENTE_GATEWAY_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_ASSINATURA_GATEWAY", "\"GATEWAY\" IN (1)");

      table.HasCheckConstraint("CK_ASSINATURA_IDENTIFICADOR_EXTERNO", "length(btrim(\"IDENTIFICADOR_EXTERNO\")) > 0");

      table.HasCheckConstraint("CK_ASSINATURA_STATUS", "\"STATUS\" IN (1, 2, 3, 4, 5)");

      table.HasCheckConstraint("CK_ASSINATURA_VALOR_CONTRATADO", "\"VALOR_CONTRATADO\" >= 0.01 AND \"VALOR_CONTRATADO\" <= 999999999999.99");

      table.HasCheckConstraint("CK_ASSINATURA_MOEDA", "length(btrim(\"MOEDA\")) > 0");
    });

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
           .HasColumnName("ID")
           .HasColumnType("uuid")
           .IsRequired()
           .ValueGeneratedNever();

    builder.Property(x => x.ContaId)
           .HasColumnName("CONTA_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.PlanoId)
           .HasColumnName("PLANO_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.ClienteGatewayId)
           .HasColumnName("CLIENTE_GATEWAY_ID")
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

    builder.Property(x => x.Status)
           .HasColumnName("STATUS")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.ValorContratado)
           .HasColumnName("VALOR_CONTRATADO")
           .HasPrecision(14, 2)
           .HasColumnType("numeric(14,2)")
           .IsRequired();

    builder.Property(x => x.Moeda)
           .HasColumnName("MOEDA")
           .HasMaxLength(3)
           .IsFixedLength()
           .HasColumnType("char(3)")
           .IsRequired();

    builder.Property(x => x.PeriodoIniciadoEm)
           .HasColumnName("PERIODO_INICIADO_EM")
           .HasColumnType("timestamp with time zone");

    builder.Property(x => x.PeriodoTerminaEm)
           .HasColumnName("PERIODO_TERMINA_EM")
           .HasColumnType("timestamp with time zone");

    builder.Property(x => x.CancelamentoSolicitadoEm)
           .HasColumnName("CANCELAMENTO_SOLICITADO_EM")
           .HasColumnType("timestamp with time zone");

    builder.Property(x => x.CanceladaEm)
           .HasColumnName("CANCELADA_EM")
           .HasColumnType("timestamp with time zone");

    builder.Property(x => x.CriadaEm)
           .HasColumnName("CRIADA_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Property(x => x.AtualizadaEm)
           .HasColumnName("ATUALIZADA_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.HasOne(x => x.Plano)
           .WithMany(x => x.Assinaturas)
           .HasForeignKey(x => x.PlanoId)
           .OnDelete(DeleteBehavior.Restrict);

    builder.HasOne(x => x.ClienteGateway)
           .WithMany(x => x.Assinaturas)
           .HasForeignKey(x => new { x.ClienteGatewayId, x.ContaId, x.Gateway })
           .HasPrincipalKey(x => new { x.Id, x.ContaId, x.Gateway })
           .OnDelete(DeleteBehavior.Restrict);

    builder.Navigation(x => x.Pagamentos)
           .HasField("_pagamentos")
           .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.HasIndex(x => new { x.Gateway, x.IdentificadorExterno })
           .IsUnique();
  }
}
