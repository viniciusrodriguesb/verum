using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Catalogo.Dominio.Categoria;

namespace Verum.Modules.Catalogo.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class CategoriaMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("categoria", table =>
    {
      table.HasCheckConstraint("ck_categoria_id", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_categoria_categoria_pai_id", "categoria_pai_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_categoria_nome", "length(btrim(nome)) > 0");
      table.HasCheckConstraint("ck_categoria_slug", "length(btrim(slug)) > 0");
      table.HasCheckConstraint("ck_categoria_regra_4", "categoria_pai_id IS NULL OR categoria_pai_id <> id");
    });
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
    builder.Property(x => x.CategoriaPaiId).HasColumnName("categoria_pai_id").HasColumnType("uuid");
    builder.Property(x => x.Nome).HasColumnName("nome").HasMaxLength(120).HasColumnType("varchar(120)").IsRequired();
    builder.Property(x => x.Slug).HasColumnName("slug").HasMaxLength(140).HasColumnType("varchar(140)").IsRequired();
    builder.Property(x => x.Ativa).HasColumnName("ativa").HasColumnType("boolean").IsRequired();
    builder.Property(x => x.CriadaEm).HasColumnName("criada_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Property(x => x.AtualizadaEm).HasColumnName("atualizada_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.HasOne(x => x.CategoriaPai).WithMany(x => x.Subcategorias).HasForeignKey(x => x.CategoriaPaiId).OnDelete(DeleteBehavior.Restrict);
    builder.Navigation(x => x.Subcategorias).HasField("_subcategorias").UsePropertyAccessMode(PropertyAccessMode.Field);
    builder.Navigation(x => x.Produtos).HasField("_produtos").UsePropertyAccessMode(PropertyAccessMode.Field);
    builder.HasIndex(x => x.Slug).IsUnique();
  }
}

