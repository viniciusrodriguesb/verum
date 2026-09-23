using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Assinaturas.Dominio.Plano;

namespace Verum.Modules.Assinaturas.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class PlanoMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("PLANO", table =>
    {
      table.HasCheckConstraint("CK_PLANO_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_PLANO_CODIGO", "length(btrim(\"CODIGO\")) > 0");

      table.HasCheckConstraint("CK_PLANO_NOME", "length(btrim(\"NOME\")) > 0");

      table.HasCheckConstraint("CK_PLANO_DESCRICAO", "length(btrim(\"DESCRICAO\")) > 0");

      table.HasCheckConstraint("CK_PLANO_PACOTE_ACESSO_CODIGO", "length(btrim(\"PACOTE_ACESSO_CODIGO\")) > 0");

      table.HasCheckConstraint("CK_PLANO_VALOR_ATUAL", "\"VALOR_ATUAL\" >= 0.01 AND \"VALOR_ATUAL\" <= 999999999999.99");

      table.HasCheckConstraint("CK_PLANO_MOEDA", "length(btrim(\"MOEDA\")) > 0");

      table.HasCheckConstraint("CK_PLANO_PERIODICIDADE", "\"PERIODICIDADE\" IN (1, 2)");
    });

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
           .HasColumnName("ID")
           .HasColumnType("uuid")
           .IsRequired()
           .ValueGeneratedNever();

    builder.Property(x => x.Codigo)
           .HasColumnName("CODIGO")
           .HasMaxLength(80)
           .HasColumnType("varchar(80)")
           .IsRequired();

    builder.Property(x => x.Nome)
           .HasColumnName("NOME")
           .HasMaxLength(120)
           .HasColumnType("varchar(120)")
           .IsRequired();

    builder.Property(x => x.Descricao)
           .HasColumnName("DESCRICAO")
           .HasMaxLength(500)
           .HasColumnType("varchar(500)")
           .IsRequired();

    builder.Property(x => x.PacoteAcessoCodigo)
           .HasColumnName("PACOTE_ACESSO_CODIGO")
           .HasMaxLength(80)
           .HasColumnType("varchar(80)")
           .IsRequired();

    builder.Property(x => x.ValorAtual)
           .HasColumnName("VALOR_ATUAL")
           .HasPrecision(14, 2)
           .HasColumnType("numeric(14,2)")
           .IsRequired();

    builder.Property(x => x.Moeda)
           .HasColumnName("MOEDA")
           .HasMaxLength(3)
           .IsFixedLength()
           .HasColumnType("char(3)")
           .IsRequired();

    builder.Property(x => x.Periodicidade)
           .HasColumnName("PERIODICIDADE")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.Ativo)
           .HasColumnName("ATIVO")
           .HasColumnType("boolean")
           .IsRequired();

    builder.Property(x => x.CriadoEm)
           .HasColumnName("CRIADO_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Property(x => x.AtualizadoEm)
           .HasColumnName("ATUALIZADO_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Navigation(x => x.Assinaturas)
           .HasField("_assinaturas")
           .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.HasIndex(x => x.Codigo)
           .IsUnique();
  }
}

