using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Notificacoes.Dominio.Notificacao;

namespace Verum.Modules.Notificacoes.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class NotificacaoMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("NOTIFICACAO", table =>
    {
      table.HasCheckConstraint("CK_NOTIFICACAO_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_NOTIFICACAO_CONTA_ID", "\"CONTA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_NOTIFICACAO_TIPO", "\"TIPO\" IN (1, 2)");

      table.HasCheckConstraint("CK_NOTIFICACAO_TITULO", "length(btrim(\"TITULO\")) > 0");

      table.HasCheckConstraint("CK_NOTIFICACAO_MENSAGEM", "length(btrim(\"MENSAGEM\")) > 0");

      table.HasCheckConstraint("CK_NOTIFICACAO_DADOS", "jsonb_typeof(\"DADOS\") = 'object'");

      table.HasCheckConstraint("CK_NOTIFICACAO_STATUS", "\"STATUS\" IN (1, 2)");
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

    builder.Property(x => x.Tipo)
           .HasColumnName("TIPO")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.Titulo)
           .HasColumnName("TITULO")
           .HasMaxLength(200)
           .HasColumnType("varchar(200)")
           .IsRequired();

    builder.Property(x => x.Mensagem)
           .HasColumnName("MENSAGEM")
           .HasMaxLength(1000)
           .HasColumnType("varchar(1000)")
           .IsRequired();

    builder.Property(x => x.Dados)
           .HasColumnName("DADOS")
           .HasColumnType("jsonb");

    builder.Property(x => x.Status)
           .HasColumnName("STATUS")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.CriadaEm)
           .HasColumnName("CRIADA_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Property(x => x.LidaEm)
           .HasColumnName("LIDA_EM")
           .HasColumnType("timestamp with time zone");

    builder.Navigation(x => x.Entregas)
           .HasField("_entregas")
           .UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}

