using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Acesso.Dominio.PacoteAcesso;

namespace Verum.Modules.Acesso.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class PacoteAcessoMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("pacote_acesso", table =>
    {
      table.HasCheckConstraint("ck_pacote_acesso_id", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_pacote_acesso_codigo", "length(btrim(codigo)) > 0");
      table.HasCheckConstraint("ck_pacote_acesso_nome", "length(btrim(nome)) > 0");
    });
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
    builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(80).HasColumnType("varchar(80)").IsRequired();
    builder.Property(x => x.Nome).HasColumnName("nome").HasMaxLength(120).HasColumnType("varchar(120)").IsRequired();
    builder.Property(x => x.Ativo).HasColumnName("ativo").HasColumnType("boolean").IsRequired();
    builder.Property(x => x.CriadoEm).HasColumnName("criado_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Property(x => x.AtualizadoEm).HasColumnName("atualizado_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Navigation(x => x.Recursos).HasField("_recursos").UsePropertyAccessMode(PropertyAccessMode.Field);
    builder.Navigation(x => x.Concessoes).HasField("_concessoes").UsePropertyAccessMode(PropertyAccessMode.Field);
    builder.HasIndex(x => x.Codigo).IsUnique();
  }
}

