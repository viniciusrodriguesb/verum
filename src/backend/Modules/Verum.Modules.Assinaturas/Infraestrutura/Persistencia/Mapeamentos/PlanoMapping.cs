using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Assinaturas.Dominio.Plano;

namespace Verum.Modules.Assinaturas.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class PlanoMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("plano", table =>
    {
      table.HasCheckConstraint("ck_plano_id", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_plano_codigo", "length(btrim(codigo)) > 0");
      table.HasCheckConstraint("ck_plano_nome", "length(btrim(nome)) > 0");
      table.HasCheckConstraint("ck_plano_descricao", "length(btrim(descricao)) > 0");
      table.HasCheckConstraint("ck_plano_pacote_acesso_codigo", "length(btrim(pacote_acesso_codigo)) > 0");
      table.HasCheckConstraint("ck_plano_valor_atual", "valor_atual >= 0.01 AND valor_atual <= 999999999999.99");
      table.HasCheckConstraint("ck_plano_moeda", "length(btrim(moeda)) > 0");
      table.HasCheckConstraint("ck_plano_periodicidade", "periodicidade IN (1, 2)");
    });
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
    builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(80).HasColumnType("varchar(80)").IsRequired();
    builder.Property(x => x.Nome).HasColumnName("nome").HasMaxLength(120).HasColumnType("varchar(120)").IsRequired();
    builder.Property(x => x.Descricao).HasColumnName("descricao").HasMaxLength(500).HasColumnType("varchar(500)").IsRequired();
    builder.Property(x => x.PacoteAcessoCodigo).HasColumnName("pacote_acesso_codigo").HasMaxLength(80).HasColumnType("varchar(80)").IsRequired();
    builder.Property(x => x.ValorAtual).HasColumnName("valor_atual").HasPrecision(14, 2).HasColumnType("numeric(14,2)").IsRequired();
    builder.Property(x => x.Moeda).HasColumnName("moeda").HasMaxLength(3).IsFixedLength().HasColumnType("char(3)").IsRequired();
    builder.Property(x => x.Periodicidade).HasColumnName("periodicidade").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.Ativo).HasColumnName("ativo").HasColumnType("boolean").IsRequired();
    builder.Property(x => x.CriadoEm).HasColumnName("criado_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Property(x => x.AtualizadoEm).HasColumnName("atualizado_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Navigation(x => x.Assinaturas).HasField("_assinaturas").UsePropertyAccessMode(PropertyAccessMode.Field);
    builder.HasIndex(x => x.Codigo).IsUnique();
  }
}

