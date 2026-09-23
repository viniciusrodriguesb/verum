using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Acesso.Dominio.PacoteAcesso;

namespace Verum.Modules.Acesso.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class PacoteAcessoMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("PACOTE_ACESSO", table =>
    {
      table.HasCheckConstraint("CK_PACOTE_ACESSO_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_PACOTE_ACESSO_CODIGO", "length(btrim(\"CODIGO\")) > 0");

      table.HasCheckConstraint("CK_PACOTE_ACESSO_NOME", "length(btrim(\"NOME\")) > 0");
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

    builder.Navigation(x => x.Recursos)
           .HasField("_recursos")
           .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.Navigation(x => x.Concessoes)
           .HasField("_concessoes")
           .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.HasIndex(x => x.Codigo)
           .IsUnique();
  }
}

