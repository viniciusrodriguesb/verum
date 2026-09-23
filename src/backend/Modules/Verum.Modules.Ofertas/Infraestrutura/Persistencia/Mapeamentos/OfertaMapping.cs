using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Ofertas.Dominio.Oferta;

namespace Verum.Modules.Ofertas.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class OfertaMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("OFERTA", table =>
    {
      table.HasCheckConstraint("CK_OFERTA_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_OFERTA_PRODUTO_VARIANTE_ID", "\"PRODUTO_VARIANTE_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_OFERTA_LOJA_ID", "\"LOJA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_OFERTA_FONTE_ID", "\"FONTE_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_OFERTA_IDENTIFICADOR_EXTERNO", "length(btrim(\"IDENTIFICADOR_EXTERNO\")) > 0");

      table.HasCheckConstraint("CK_OFERTA_TITULO_EXTERNO", "length(btrim(\"TITULO_EXTERNO\")) > 0");

      table.HasCheckConstraint("CK_OFERTA_URL", "length(btrim(\"URL\")) > 0");

      table.HasCheckConstraint("CK_OFERTA_URL_HASH", "length(btrim(\"URL_HASH\")) > 0");

      table.HasCheckConstraint("CK_OFERTA_PRECO_ATUAL", "\"PRECO_ATUAL\" >= 0.01 AND \"PRECO_ATUAL\" <= 999999999999.99");

      table.HasCheckConstraint("CK_OFERTA_PRECO_PIX", "\"PRECO_PIX\" >= 0.01 AND \"PRECO_PIX\" <= 999999999999.99");

      table.HasCheckConstraint("CK_OFERTA_PRECO_ANTERIOR", "\"PRECO_ANTERIOR\" >= 0.01 AND \"PRECO_ANTERIOR\" <= 999999999999.99");

      table.HasCheckConstraint("CK_OFERTA_QUANTIDADE_PARCELAS", "\"QUANTIDADE_PARCELAS\" >= 1 AND \"QUANTIDADE_PARCELAS\" <= 32767");

      table.HasCheckConstraint("CK_OFERTA_VALOR_PARCELA", "\"VALOR_PARCELA\" >= 0.01 AND \"VALOR_PARCELA\" <= 999999999999.99");

      table.HasCheckConstraint("CK_OFERTA_CONDICAO_PRECO", "length(btrim(\"CONDICAO_PRECO\")) > 0");

      table.HasCheckConstraint("CK_OFERTA_DISPONIBILIDADE", "\"DISPONIBILIDADE\" IN (1, 2, 3)");

      table.HasCheckConstraint("CK_OFERTA_CONDICAO_PRODUTO", "\"CONDICAO_PRODUTO\" IN (1, 2, 3)");

      table.HasCheckConstraint("CK_OFERTA_IMAGEM_URL", "length(btrim(\"IMAGEM_URL\")) > 0");

      table.HasCheckConstraint("CK_OFERTA_STATUS", "\"STATUS\" IN (1, 2, 3)");

      table.HasCheckConstraint("CK_OFERTA_REGRA_18", "\"VALIDA_ATE\" > \"OBSERVADA_EM\"");

      table.HasCheckConstraint("CK_OFERTA_REGRA_19", "(\"QUANTIDADE_PARCELAS\" IS NULL) = (\"VALOR_PARCELA\" IS NULL)");
    });

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
           .HasColumnName("ID")
           .HasColumnType("uuid")
           .IsRequired()
           .ValueGeneratedNever();

    builder.Property(x => x.ProdutoVarianteId)
           .HasColumnName("PRODUTO_VARIANTE_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.LojaId)
           .HasColumnName("LOJA_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.FonteId)
           .HasColumnName("FONTE_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.IdentificadorExterno)
           .HasColumnName("IDENTIFICADOR_EXTERNO")
           .HasMaxLength(250)
           .HasColumnType("varchar(250)");

    builder.Property(x => x.TituloExterno)
           .HasColumnName("TITULO_EXTERNO")
           .HasMaxLength(500)
           .HasColumnType("varchar(500)")
           .IsRequired();

    builder.Property(x => x.Url)
           .HasColumnName("URL")
           .HasColumnType("text")
           .IsRequired();

    builder.Property(x => x.UrlHash)
           .HasColumnName("URL_HASH")
           .HasMaxLength(64)
           .HasColumnType("varchar(64)")
           .IsRequired();

    builder.Property(x => x.PrecoAtual)
           .HasColumnName("PRECO_ATUAL")
           .HasPrecision(14, 2)
           .HasColumnType("numeric(14,2)")
           .IsRequired();

    builder.Property(x => x.PrecoPix)
           .HasColumnName("PRECO_PIX")
           .HasPrecision(14, 2)
           .HasColumnType("numeric(14,2)");

    builder.Property(x => x.PrecoAnterior)
           .HasColumnName("PRECO_ANTERIOR")
           .HasPrecision(14, 2)
           .HasColumnType("numeric(14,2)");

    builder.Property(x => x.QuantidadeParcelas)
           .HasColumnName("QUANTIDADE_PARCELAS")
           .HasColumnType("smallint");

    builder.Property(x => x.ValorParcela)
           .HasColumnName("VALOR_PARCELA")
           .HasPrecision(14, 2)
           .HasColumnType("numeric(14,2)");

    builder.Property(x => x.CondicaoPreco)
           .HasColumnName("CONDICAO_PRECO")
           .HasMaxLength(300)
           .HasColumnType("varchar(300)");

    builder.Property(x => x.Disponibilidade)
           .HasColumnName("DISPONIBILIDADE")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.CondicaoProduto)
           .HasColumnName("CONDICAO_PRODUTO")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.ImagemUrl)
           .HasColumnName("IMAGEM_URL")
           .HasColumnType("text");

    builder.Property(x => x.ObservadaEm)
           .HasColumnName("OBSERVADA_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Property(x => x.ValidaAte)
           .HasColumnName("VALIDA_ATE")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Property(x => x.UltimaConfirmacaoEm)
           .HasColumnName("ULTIMA_CONFIRMACAO_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Property(x => x.Status)
           .HasColumnName("STATUS")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.CriadaEm)
           .HasColumnName("CRIADA_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Property(x => x.AtualizadaEm)
           .HasColumnName("ATUALIZADA_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.HasOne(x => x.Loja)
           .WithMany(x => x.Ofertas)
           .HasForeignKey(x => x.LojaId)
           .OnDelete(DeleteBehavior.Restrict);

    builder.HasOne(x => x.Fonte)
           .WithMany(x => x.Ofertas)
           .HasForeignKey(x => x.FonteId)
           .OnDelete(DeleteBehavior.Restrict);

    builder.Navigation(x => x.Observacoes)
           .HasField("_observacoes")
           .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.HasIndex(x => new { x.LojaId, x.UrlHash })
           .IsUnique();
  }
}

