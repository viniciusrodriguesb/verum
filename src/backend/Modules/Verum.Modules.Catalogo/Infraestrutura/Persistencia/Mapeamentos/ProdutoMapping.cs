using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Catalogo.Dominio.Produto;

namespace Verum.Modules.Catalogo.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class ProdutoMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("produto", table =>
    {
      table.HasCheckConstraint("ck_produto_id", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_produto_categoria_id", "categoria_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_produto_marca_id", "marca_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_produto_nome", "length(btrim(nome)) > 0");
      table.HasCheckConstraint("ck_produto_nome_normalizado", "length(btrim(nome_normalizado)) > 0");
      table.HasCheckConstraint("ck_produto_modelo", "length(btrim(modelo)) > 0");
      table.HasCheckConstraint("ck_produto_slug", "length(btrim(slug)) > 0");
      table.HasCheckConstraint("ck_produto_descricao", "length(btrim(descricao)) > 0");
      table.HasCheckConstraint("ck_produto_atributos", "jsonb_typeof(atributos) = 'object'");
      table.HasCheckConstraint("ck_produto_status", "status IN (1, 2, 3)");
    });
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
    builder.Property(x => x.CategoriaId).HasColumnName("categoria_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.MarcaId).HasColumnName("marca_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.Nome).HasColumnName("nome").HasMaxLength(250).HasColumnType("varchar(250)").IsRequired();
    builder.Property(x => x.NomeNormalizado).HasColumnName("nome_normalizado").HasMaxLength(250).HasColumnType("varchar(250)").IsRequired();
    builder.Property(x => x.Modelo).HasColumnName("modelo").HasMaxLength(120).HasColumnType("varchar(120)");
    builder.Property(x => x.Slug).HasColumnName("slug").HasMaxLength(280).HasColumnType("varchar(280)").IsRequired();
    builder.Property(x => x.Descricao).HasColumnName("descricao").HasColumnType("text");
    builder.Property(x => x.Atributos).HasColumnName("atributos").HasColumnType("jsonb").IsRequired();
    builder.Property(x => x.Status).HasColumnName("status").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.CriadoEm).HasColumnName("criado_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Property(x => x.AtualizadoEm).HasColumnName("atualizado_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.HasOne(x => x.Categoria).WithMany(x => x.Produtos).HasForeignKey(x => x.CategoriaId).OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.Marca).WithMany(x => x.Produtos).HasForeignKey(x => x.MarcaId).OnDelete(DeleteBehavior.Restrict);
    builder.Navigation(x => x.Variantes).HasField("_variantes").UsePropertyAccessMode(PropertyAccessMode.Field);
    builder.Navigation(x => x.TermosBusca).HasField("_termosBusca").UsePropertyAccessMode(PropertyAccessMode.Field);
    builder.Navigation(x => x.Imagens).HasField("_imagens").UsePropertyAccessMode(PropertyAccessMode.Field);
    builder.HasIndex(x => x.Slug).IsUnique();
  }
}

