using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Assinaturas.Dominio.EventoGateway;

namespace Verum.Modules.Assinaturas.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class EventoGatewayMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("EVENTO_GATEWAY", table =>
    {
      table.HasCheckConstraint("CK_EVENTO_GATEWAY_GATEWAY", "\"GATEWAY\" IN (1)");

      table.HasCheckConstraint("CK_EVENTO_GATEWAY_IDENTIFICADOR_EXTERNO", "length(btrim(\"IDENTIFICADOR_EXTERNO\")) > 0");

      table.HasCheckConstraint("CK_EVENTO_GATEWAY_TIPO", "length(btrim(\"TIPO\")) > 0");

      table.HasCheckConstraint("CK_EVENTO_GATEWAY_CONTEUDO", "jsonb_typeof(\"CONTEUDO\") = 'object'");

      table.HasCheckConstraint("CK_EVENTO_GATEWAY_STATUS", "\"STATUS\" IN (1, 2, 3)");

      table.HasCheckConstraint("CK_EVENTO_GATEWAY_TENTATIVAS", "\"TENTATIVAS\" >= 0 AND \"TENTATIVAS\" <= 32767");

      table.HasCheckConstraint("CK_EVENTO_GATEWAY_ULTIMO_ERRO", "length(btrim(\"ULTIMO_ERRO\")) > 0");
    });

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
           .HasColumnName("ID")
           .HasColumnType("bigint")
           .IsRequired()
           .UseIdentityByDefaultColumn();

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

    builder.Property(x => x.Tipo)
           .HasColumnName("TIPO")
           .HasMaxLength(150)
           .HasColumnType("varchar(150)")
           .IsRequired();

    builder.Property(x => x.Conteudo)
           .HasColumnName("CONTEUDO")
           .HasColumnType("jsonb")
           .IsRequired();

    builder.Property(x => x.Status)
           .HasColumnName("STATUS")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.RecebidoEm)
           .HasColumnName("RECEBIDO_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Property(x => x.ProcessadoEm)
           .HasColumnName("PROCESSADO_EM")
           .HasColumnType("timestamp with time zone");

    builder.Property(x => x.Tentativas)
           .HasColumnName("TENTATIVAS")
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.UltimoErro)
           .HasColumnName("ULTIMO_ERRO")
           .HasMaxLength(1000)
           .HasColumnType("varchar(1000)");

    builder.HasIndex(x => new { x.Gateway, x.IdentificadorExterno })
           .IsUnique();
  }
}

