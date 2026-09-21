using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Ofertas.Dominio.Oferta;

namespace Verum.Modules.Ofertas.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class OfertaMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("oferta", table =>
    {
      table.HasCheckConstraint("ck_oferta_id", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_oferta_produto_variante_id", "produto_variante_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_oferta_loja_id", "loja_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_oferta_fonte_id", "fonte_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_oferta_identificador_externo", "length(btrim(identificador_externo)) > 0");
      table.HasCheckConstraint("ck_oferta_titulo_externo", "length(btrim(titulo_externo)) > 0");
      table.HasCheckConstraint("ck_oferta_url", "length(btrim(url)) > 0");
      table.HasCheckConstraint("ck_oferta_url_hash", "length(btrim(url_hash)) > 0");
      table.HasCheckConstraint("ck_oferta_preco_atual", "preco_atual >= 0.01 AND preco_atual <= 999999999999.99");
      table.HasCheckConstraint("ck_oferta_preco_pix", "preco_pix >= 0.01 AND preco_pix <= 999999999999.99");
      table.HasCheckConstraint("ck_oferta_preco_anterior", "preco_anterior >= 0.01 AND preco_anterior <= 999999999999.99");
      table.HasCheckConstraint("ck_oferta_quantidade_parcelas", "quantidade_parcelas >= 1 AND quantidade_parcelas <= 32767");
      table.HasCheckConstraint("ck_oferta_valor_parcela", "valor_parcela >= 0.01 AND valor_parcela <= 999999999999.99");
      table.HasCheckConstraint("ck_oferta_condicao_preco", "length(btrim(condicao_preco)) > 0");
      table.HasCheckConstraint("ck_oferta_disponibilidade", "disponibilidade IN (1, 2, 3)");
      table.HasCheckConstraint("ck_oferta_condicao_produto", "condicao_produto IN (1, 2, 3)");
      table.HasCheckConstraint("ck_oferta_imagem_url", "length(btrim(imagem_url)) > 0");
      table.HasCheckConstraint("ck_oferta_status", "status IN (1, 2, 3)");
      table.HasCheckConstraint("ck_oferta_regra_18", "valida_ate > observada_em");
      table.HasCheckConstraint("ck_oferta_regra_19", "(quantidade_parcelas IS NULL) = (valor_parcela IS NULL)");
    });
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
    builder.Property(x => x.ProdutoVarianteId).HasColumnName("produto_variante_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.LojaId).HasColumnName("loja_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.FonteId).HasColumnName("fonte_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.IdentificadorExterno).HasColumnName("identificador_externo").HasMaxLength(250).HasColumnType("varchar(250)");
    builder.Property(x => x.TituloExterno).HasColumnName("titulo_externo").HasMaxLength(500).HasColumnType("varchar(500)").IsRequired();
    builder.Property(x => x.Url).HasColumnName("url").HasColumnType("text").IsRequired();
    builder.Property(x => x.UrlHash).HasColumnName("url_hash").HasMaxLength(64).HasColumnType("varchar(64)").IsRequired();
    builder.Property(x => x.PrecoAtual).HasColumnName("preco_atual").HasPrecision(14, 2).HasColumnType("numeric(14,2)").IsRequired();
    builder.Property(x => x.PrecoPix).HasColumnName("preco_pix").HasPrecision(14, 2).HasColumnType("numeric(14,2)");
    builder.Property(x => x.PrecoAnterior).HasColumnName("preco_anterior").HasPrecision(14, 2).HasColumnType("numeric(14,2)");
    builder.Property(x => x.QuantidadeParcelas).HasColumnName("quantidade_parcelas").HasColumnType("smallint");
    builder.Property(x => x.ValorParcela).HasColumnName("valor_parcela").HasPrecision(14, 2).HasColumnType("numeric(14,2)");
    builder.Property(x => x.CondicaoPreco).HasColumnName("condicao_preco").HasMaxLength(300).HasColumnType("varchar(300)");
    builder.Property(x => x.Disponibilidade).HasColumnName("disponibilidade").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.CondicaoProduto).HasColumnName("condicao_produto").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.ImagemUrl).HasColumnName("imagem_url").HasColumnType("text");
    builder.Property(x => x.ObservadaEm).HasColumnName("observada_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Property(x => x.ValidaAte).HasColumnName("valida_ate").HasColumnType("timestamp with time zone").IsRequired();
    builder.Property(x => x.UltimaConfirmacaoEm).HasColumnName("ultima_confirmacao_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Property(x => x.Status).HasColumnName("status").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.CriadaEm).HasColumnName("criada_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Property(x => x.AtualizadaEm).HasColumnName("atualizada_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.HasOne(x => x.Loja).WithMany(x => x.Ofertas).HasForeignKey(x => x.LojaId).OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.Fonte).WithMany(x => x.Ofertas).HasForeignKey(x => x.FonteId).OnDelete(DeleteBehavior.Restrict);
    builder.Navigation(x => x.Observacoes).HasField("_observacoes").UsePropertyAccessMode(PropertyAccessMode.Field);
    builder.HasIndex(x => new { x.LojaId, x.UrlHash }).IsUnique();
  }
}

