using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Busca.Dominio.ResultadoOferta;

namespace Verum.Modules.Busca.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class ResultadoOfertaMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("resultado_oferta", table =>
    {
      table.HasCheckConstraint("ck_resultado_oferta_resultado_busca_id", "resultado_busca_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_resultado_oferta_oferta_id", "oferta_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_resultado_oferta_produto_variante_id", "produto_variante_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_resultado_oferta_posicao", "posicao >= 1 AND posicao <= 32767");
      table.HasCheckConstraint("ck_resultado_oferta_classificacao", "classificacao IN (1, 2, 3)");
      table.HasCheckConstraint("ck_resultado_oferta_pontuacao_final", "pontuacao_final >= 0 AND pontuacao_final <= 100");
      table.HasCheckConstraint("ck_resultado_oferta_pontuacao_preco", "pontuacao_preco >= 0 AND pontuacao_preco <= 100");
      table.HasCheckConstraint("ck_resultado_oferta_pontuacao_confianca", "pontuacao_confianca >= 0 AND pontuacao_confianca <= 100");
      table.HasCheckConstraint("ck_resultado_oferta_pontuacao_atualizacao", "pontuacao_atualizacao >= 0 AND pontuacao_atualizacao <= 100");
      table.HasCheckConstraint("ck_resultado_oferta_explicacao", "length(btrim(explicacao)) > 0");
      table.HasCheckConstraint("ck_resultado_oferta_oferta_snapshot", "jsonb_typeof(oferta_snapshot) = 'object'");
    });
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").IsRequired().UseIdentityByDefaultColumn();
    builder.Property(x => x.ResultadoBuscaId).HasColumnName("resultado_busca_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.OfertaId).HasColumnName("oferta_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.ProdutoVarianteId).HasColumnName("produto_variante_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.Posicao).HasColumnName("posicao").HasColumnType("smallint").IsRequired();
    builder.Property(x => x.Classificacao).HasColumnName("classificacao").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.PontuacaoFinal).HasColumnName("pontuacao_final").HasPrecision(5, 2).HasColumnType("numeric(5,2)").IsRequired();
    builder.Property(x => x.PontuacaoPreco).HasColumnName("pontuacao_preco").HasPrecision(5, 2).HasColumnType("numeric(5,2)").IsRequired();
    builder.Property(x => x.PontuacaoConfianca).HasColumnName("pontuacao_confianca").HasPrecision(5, 2).HasColumnType("numeric(5,2)").IsRequired();
    builder.Property(x => x.PontuacaoAtualizacao).HasColumnName("pontuacao_atualizacao").HasPrecision(5, 2).HasColumnType("numeric(5,2)").IsRequired();
    builder.Property(x => x.Explicacao).HasColumnName("explicacao").HasMaxLength(1000).HasColumnType("varchar(1000)").IsRequired();
    builder.Property(x => x.OfertaSnapshot).HasColumnName("oferta_snapshot").HasColumnType("jsonb").IsRequired();
    builder.HasOne(x => x.Resultado).WithMany(x => x.Ofertas).HasForeignKey(x => x.ResultadoBuscaId).OnDelete(DeleteBehavior.Restrict);
    builder.HasIndex(x => new { x.ResultadoBuscaId, x.Posicao }).IsUnique();
  }
}

