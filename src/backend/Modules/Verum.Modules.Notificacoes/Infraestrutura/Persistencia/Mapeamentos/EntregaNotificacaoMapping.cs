using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Notificacoes.Dominio.EntregaNotificacao;

namespace Verum.Modules.Notificacoes.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class EntregaNotificacaoMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("ENTREGA_NOTIFICACAO", table =>
    {
      table.HasCheckConstraint("CK_ENTREGA_NOTIFICACAO_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_ENTREGA_NOTIFICACAO_NOTIFICACAO_ID", "\"NOTIFICACAO_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_ENTREGA_NOTIFICACAO_CANAL", "\"CANAL\" IN (1, 2, 3)");

      table.HasCheckConstraint("CK_ENTREGA_NOTIFICACAO_STATUS", "\"STATUS\" IN (1, 2, 3, 4, 5, 6)");

      table.HasCheckConstraint("CK_ENTREGA_NOTIFICACAO_QUANTIDADE_TENTATIVAS", "\"QUANTIDADE_TENTATIVAS\" >= 0 AND \"QUANTIDADE_TENTATIVAS\" <= 32767");

      table.HasCheckConstraint("CK_ENTREGA_NOTIFICACAO_CODIGO_ERRO", "length(btrim(\"CODIGO_ERRO\")) > 0");

      table.HasCheckConstraint("CK_ENTREGA_NOTIFICACAO_DETALHES_ERRO", "length(btrim(\"DETALHES_ERRO\")) > 0");

      table.HasCheckConstraint("CK_ENTREGA_NOTIFICACAO_REFERENCIA_PROVEDOR", "length(btrim(\"REFERENCIA_PROVEDOR\")) > 0");
    });

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
           .HasColumnName("ID")
           .HasColumnType("uuid")
           .IsRequired()
           .ValueGeneratedNever();

    builder.Property(x => x.NotificacaoId)
           .HasColumnName("NOTIFICACAO_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.Canal)
           .HasColumnName("CANAL")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.Status)
           .HasColumnName("STATUS")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.QuantidadeTentativas)
           .HasColumnName("QUANTIDADE_TENTATIVAS")
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.ProximaTentativaEm)
           .HasColumnName("PROXIMA_TENTATIVA_EM")
           .HasColumnType("timestamp with time zone");

    builder.Property(x => x.UltimaTentativaEm)
           .HasColumnName("ULTIMA_TENTATIVA_EM")
           .HasColumnType("timestamp with time zone");

    builder.Property(x => x.EntregueEm)
           .HasColumnName("ENTREGUE_EM")
           .HasColumnType("timestamp with time zone");

    builder.Property(x => x.CodigoErro)
           .HasColumnName("CODIGO_ERRO")
           .HasMaxLength(100)
           .HasColumnType("varchar(100)");

    builder.Property(x => x.DetalhesErro)
           .HasColumnName("DETALHES_ERRO")
           .HasMaxLength(1000)
           .HasColumnType("varchar(1000)");

    builder.Property(x => x.ReferenciaProvedor)
           .HasColumnName("REFERENCIA_PROVEDOR")
           .HasMaxLength(200)
           .HasColumnType("varchar(200)");

    builder.HasOne(x => x.Notificacao)
           .WithMany(x => x.Entregas)
           .HasForeignKey(x => x.NotificacaoId)
           .OnDelete(DeleteBehavior.Restrict);

    builder.HasIndex(x => new { x.NotificacaoId, x.Canal })
           .IsUnique();
  }
}

