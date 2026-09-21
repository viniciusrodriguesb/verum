using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Catalogo.Dominio.Marca;

namespace Verum.Modules.Catalogo.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class MarcaMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("marca", table =>
    {
      table.HasCheckConstraint("ck_marca_id", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_marca_nome", "length(btrim(nome)) > 0");
      table.HasCheckConstraint("ck_marca_nome_normalizado", "length(btrim(nome_normalizado)) > 0");
      table.HasCheckConstraint("ck_marca_slug", "length(btrim(slug)) > 0");
    });
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
    builder.Property(x => x.Nome).HasColumnName("nome").HasMaxLength(120).HasColumnType("varchar(120)").IsRequired();
    builder.Property(x => x.NomeNormalizado).HasColumnName("nome_normalizado").HasMaxLength(120).HasColumnType("varchar(120)").IsRequired();
    builder.Property(x => x.Slug).HasColumnName("slug").HasMaxLength(140).HasColumnType("varchar(140)").IsRequired();
    builder.Property(x => x.Ativa).HasColumnName("ativa").HasColumnType("boolean").IsRequired();
    builder.Property(x => x.CriadaEm).HasColumnName("criada_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Navigation(x => x.Produtos).HasField("_produtos").UsePropertyAccessMode(PropertyAccessMode.Field);
    builder.HasIndex(x => x.Slug).IsUnique();
    builder.HasIndex(x => x.NomeNormalizado).IsUnique();
  }
}

