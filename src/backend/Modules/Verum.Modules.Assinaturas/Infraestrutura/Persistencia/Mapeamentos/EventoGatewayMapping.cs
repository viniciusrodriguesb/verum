using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Assinaturas.Dominio.EventoGateway;

namespace Verum.Modules.Assinaturas.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class EventoGatewayMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("evento_gateway", table =>
    {
      table.HasCheckConstraint("ck_evento_gateway_gateway", "gateway IN (1)");
      table.HasCheckConstraint("ck_evento_gateway_identificador_externo", "length(btrim(identificador_externo)) > 0");
      table.HasCheckConstraint("ck_evento_gateway_tipo", "length(btrim(tipo)) > 0");
      table.HasCheckConstraint("ck_evento_gateway_conteudo", "jsonb_typeof(conteudo) = 'object'");
      table.HasCheckConstraint("ck_evento_gateway_status", "status IN (1, 2, 3)");
      table.HasCheckConstraint("ck_evento_gateway_tentativas", "tentativas >= 0 AND tentativas <= 32767");
      table.HasCheckConstraint("ck_evento_gateway_ultimo_erro", "length(btrim(ultimo_erro)) > 0");
    });
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").IsRequired().UseIdentityByDefaultColumn();
    builder.Property(x => x.Gateway).HasColumnName("gateway").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.IdentificadorExterno).HasColumnName("identificador_externo").HasMaxLength(200).HasColumnType("varchar(200)").IsRequired();
    builder.Property(x => x.Tipo).HasColumnName("tipo").HasMaxLength(150).HasColumnType("varchar(150)").IsRequired();
    builder.Property(x => x.Conteudo).HasColumnName("conteudo").HasColumnType("jsonb").IsRequired();
    builder.Property(x => x.Status).HasColumnName("status").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.RecebidoEm).HasColumnName("recebido_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Property(x => x.ProcessadoEm).HasColumnName("processado_em").HasColumnType("timestamp with time zone");
    builder.Property(x => x.Tentativas).HasColumnName("tentativas").HasColumnType("smallint").IsRequired();
    builder.Property(x => x.UltimoErro).HasColumnName("ultimo_erro").HasMaxLength(1000).HasColumnType("varchar(1000)");
    builder.HasIndex(x => new { x.Gateway, x.IdentificadorExterno }).IsUnique();
  }
}

