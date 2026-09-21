using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Catalogo.Dominio.ProdutoVariante;

namespace Verum.Modules.Catalogo.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class ProdutoVarianteMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("produto_variante", table =>
    {
      table.HasCheckConstraint("ck_produto_variante_id", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_produto_variante_produto_id", "produto_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_produto_variante_nome", "length(btrim(nome)) > 0");
      table.HasCheckConstraint("ck_produto_variante_nome_normalizado", "length(btrim(nome_normalizado)) > 0");
      table.HasCheckConstraint("ck_produto_variante_slug", "length(btrim(slug)) > 0");
      table.HasCheckConstraint("ck_produto_variante_atributos", "jsonb_typeof(atributos) = 'object'");
      table.HasCheckConstraint("ck_produto_variante_status", "status IN (1, 2, 3)");
    });
    builder.HasKey(x => x.Id);
    builder.HasAlternateKey(x => new { x.Id, x.ProdutoId });
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
    builder.Property(x => x.ProdutoId).HasColumnName("produto_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.Nome).HasColumnName("nome").HasMaxLength(300).HasColumnType("varchar(300)").IsRequired();
    builder.Property(x => x.NomeNormalizado).HasColumnName("nome_normalizado").HasMaxLength(300).HasColumnType("varchar(300)").IsRequired();
    builder.Property(x => x.Slug).HasColumnName("slug").HasMaxLength(320).HasColumnType("varchar(320)").IsRequired();
    builder.Property(x => x.Atributos).HasColumnName("atributos").HasColumnType("jsonb").IsRequired();
    builder.Property(x => x.Status).HasColumnName("status").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.CriadaEm).HasColumnName("criada_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Property(x => x.AtualizadaEm).HasColumnName("atualizada_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.HasOne(x => x.Produto).WithMany(x => x.Variantes).HasForeignKey(x => x.ProdutoId).OnDelete(DeleteBehavior.Restrict);
    builder.Navigation(x => x.Identificadores).HasField("_identificadores").UsePropertyAccessMode(PropertyAccessMode.Field);
    builder.Navigation(x => x.TermosBusca).HasField("_termosBusca").UsePropertyAccessMode(PropertyAccessMode.Field);
    builder.Navigation(x => x.Imagens).HasField("_imagens").UsePropertyAccessMode(PropertyAccessMode.Field);
    builder.HasIndex(x => x.Slug).IsUnique();
  }
}
