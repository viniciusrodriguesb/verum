using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Ofertas.Dominio.Loja;

namespace Verum.Modules.Ofertas.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class LojaMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("loja", table =>
    {
      table.HasCheckConstraint("ck_loja_id", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_loja_nome", "length(btrim(nome)) > 0");
      table.HasCheckConstraint("ck_loja_nome_normalizado", "length(btrim(nome_normalizado)) > 0");
      table.HasCheckConstraint("ck_loja_dominio", "length(btrim(dominio)) > 0");
      table.HasCheckConstraint("ck_loja_url", "length(btrim(url)) > 0");
      table.HasCheckConstraint("ck_loja_pontuacao_confianca", "pontuacao_confianca >= 0 AND pontuacao_confianca <= 100");
      table.HasCheckConstraint("ck_loja_status", "status IN (1, 2, 3)");
    });
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
    builder.Property(x => x.Nome).HasColumnName("nome").HasMaxLength(180).HasColumnType("varchar(180)").IsRequired();
    builder.Property(x => x.NomeNormalizado).HasColumnName("nome_normalizado").HasMaxLength(180).HasColumnType("varchar(180)").IsRequired();
    builder.Property(x => x.Dominio).HasColumnName("dominio").HasMaxLength(255).HasColumnType("varchar(255)");
    builder.Property(x => x.Url).HasColumnName("url").HasColumnType("text");
    builder.Property(x => x.Verificada).HasColumnName("verificada").HasColumnType("boolean").IsRequired();
    builder.Property(x => x.PontuacaoConfianca).HasColumnName("pontuacao_confianca").HasPrecision(5, 2).HasColumnType("numeric(5,2)").IsRequired();
    builder.Property(x => x.Status).HasColumnName("status").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.CriadaEm).HasColumnName("criada_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Property(x => x.AtualizadaEm).HasColumnName("atualizada_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Navigation(x => x.Ofertas).HasField("_ofertas").UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}

