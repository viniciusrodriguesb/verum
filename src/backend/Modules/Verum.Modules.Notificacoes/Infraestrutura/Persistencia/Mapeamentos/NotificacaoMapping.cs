using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Notificacoes.Dominio.Notificacao;

namespace Verum.Modules.Notificacoes.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class NotificacaoMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("notificacao", table =>
    {
      table.HasCheckConstraint("ck_notificacao_id", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_notificacao_conta_id", "conta_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_notificacao_tipo", "tipo IN (1, 2)");
      table.HasCheckConstraint("ck_notificacao_titulo", "length(btrim(titulo)) > 0");
      table.HasCheckConstraint("ck_notificacao_mensagem", "length(btrim(mensagem)) > 0");
      table.HasCheckConstraint("ck_notificacao_dados", "jsonb_typeof(dados) = 'object'");
      table.HasCheckConstraint("ck_notificacao_status", "status IN (1, 2)");
    });
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
    builder.Property(x => x.ContaId).HasColumnName("conta_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.Tipo).HasColumnName("tipo").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.Titulo).HasColumnName("titulo").HasMaxLength(200).HasColumnType("varchar(200)").IsRequired();
    builder.Property(x => x.Mensagem).HasColumnName("mensagem").HasMaxLength(1000).HasColumnType("varchar(1000)").IsRequired();
    builder.Property(x => x.Dados).HasColumnName("dados").HasColumnType("jsonb");
    builder.Property(x => x.Status).HasColumnName("status").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.CriadaEm).HasColumnName("criada_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Property(x => x.LidaEm).HasColumnName("lida_em").HasColumnType("timestamp with time zone");
    builder.Navigation(x => x.Entregas).HasField("_entregas").UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}

