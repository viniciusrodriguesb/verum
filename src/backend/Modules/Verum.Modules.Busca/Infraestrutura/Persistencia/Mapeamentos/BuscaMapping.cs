using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Busca.Dominio.Busca;

namespace Verum.Modules.Busca.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class BuscaMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("BUSCA", table =>
    {
      table.HasCheckConstraint("CK_BUSCA_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_BUSCA_CONTA_ID", "\"CONTA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_BUSCA_VISITANTE_ID", "\"VISITANTE_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_BUSCA_CHAVE_IDEMPOTENCIA", "length(btrim(\"CHAVE_IDEMPOTENCIA\")) > 0");

      table.HasCheckConstraint("CK_BUSCA_CONSULTA_ORIGINAL", "length(btrim(\"CONSULTA_ORIGINAL\")) > 0");

      table.HasCheckConstraint("CK_BUSCA_CONSULTA_NORMALIZADA", "length(btrim(\"CONSULTA_NORMALIZADA\")) > 0");

      table.HasCheckConstraint("CK_BUSCA_CONSULTA_ESTRUTURADA", "jsonb_typeof(\"CONSULTA_ESTRUTURADA\") = 'object'");

      table.HasCheckConstraint("CK_BUSCA_PRODUTO_PRINCIPAL_ID", "\"PRODUTO_PRINCIPAL_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_BUSCA_PRODUTO_VARIANTE_PRINCIPAL_ID", "\"PRODUTO_VARIANTE_PRINCIPAL_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_BUSCA_STATUS", "\"STATUS\" IN (1, 2, 3, 4, 5, 6)");

      table.HasCheckConstraint("CK_BUSCA_ETAPA_ATUAL", "\"ETAPA_ATUAL\" IN (1, 2, 3, 4, 5, 6)");

      table.HasCheckConstraint("CK_BUSCA_MOTIVO_SEM_RESULTADO", "\"MOTIVO_SEM_RESULTADO\" IN (1, 2, 3, 4)");

      table.HasCheckConstraint("CK_BUSCA_CODIGO_ERRO", "length(btrim(\"CODIGO_ERRO\")) > 0");

      table.HasCheckConstraint("CK_BUSCA_MENSAGEM_ERRO", "length(btrim(\"MENSAGEM_ERRO\")) > 0");

      table.HasCheckConstraint("CK_BUSCA_QUANTIDADE_FONTES_CONSULTADAS", "\"QUANTIDADE_FONTES_CONSULTADAS\" >= 0 AND \"QUANTIDADE_FONTES_CONSULTADAS\" <= 2147483647");

      table.HasCheckConstraint("CK_BUSCA_QUANTIDADE_FONTES_COM_SUCESSO", "\"QUANTIDADE_FONTES_COM_SUCESSO\" >= 0 AND \"QUANTIDADE_FONTES_COM_SUCESSO\" <= 2147483647");

      table.HasCheckConstraint("CK_BUSCA_QUANTIDADE_FONTES_COM_FALHA", "\"QUANTIDADE_FONTES_COM_FALHA\" >= 0 AND \"QUANTIDADE_FONTES_COM_FALHA\" <= 2147483647");

      table.HasCheckConstraint("CK_BUSCA_REGRA_17", "(\"CONTA_ID\" IS NULL) <> (\"VISITANTE_ID\" IS NULL)");

      table.HasCheckConstraint("CK_BUSCA_REGRA_18", "\"EXPIRA_EM\" > \"INICIADA_EM\"");
    });

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
           .HasColumnName("ID")
           .HasColumnType("uuid")
           .IsRequired()
           .ValueGeneratedNever();

    builder.Property(x => x.ContaId)
           .HasColumnName("CONTA_ID")
           .HasColumnType("uuid");

    builder.Property(x => x.VisitanteId)
           .HasColumnName("VISITANTE_ID")
           .HasColumnType("uuid");

    builder.Property(x => x.ChaveIdempotencia)
           .HasColumnName("CHAVE_IDEMPOTENCIA")
           .HasMaxLength(100)
           .HasColumnType("varchar(100)")
           .IsRequired();

    builder.Property(x => x.ConsultaOriginal)
           .HasColumnName("CONSULTA_ORIGINAL")
           .HasMaxLength(500)
           .HasColumnType("varchar(500)")
           .IsRequired();

    builder.Property(x => x.ConsultaNormalizada)
           .HasColumnName("CONSULTA_NORMALIZADA")
           .HasMaxLength(500)
           .HasColumnType("varchar(500)")
           .IsRequired();

    builder.Property(x => x.ConsultaEstruturada)
           .HasColumnName("CONSULTA_ESTRUTURADA")
           .HasColumnType("jsonb");

    builder.Property(x => x.ProdutoPrincipalId)
           .HasColumnName("PRODUTO_PRINCIPAL_ID")
           .HasColumnType("uuid");

    builder.Property(x => x.ProdutoVariantePrincipalId)
           .HasColumnName("PRODUTO_VARIANTE_PRINCIPAL_ID")
           .HasColumnType("uuid");

    builder.Property(x => x.Status)
           .HasColumnName("STATUS")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.EtapaAtual)
           .HasColumnName("ETAPA_ATUAL")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.MotivoSemResultado)
           .HasColumnName("MOTIVO_SEM_RESULTADO")
           .HasConversion<short>()
           .HasColumnType("smallint");

    builder.Property(x => x.ResultadoParcial)
           .HasColumnName("RESULTADO_PARCIAL")
           .HasColumnType("boolean")
           .IsRequired();

    builder.Property(x => x.CodigoErro)
           .HasColumnName("CODIGO_ERRO")
           .HasMaxLength(100)
           .HasColumnType("varchar(100)");

    builder.Property(x => x.MensagemErro)
           .HasColumnName("MENSAGEM_ERRO")
           .HasMaxLength(500)
           .HasColumnType("varchar(500)");

    builder.Property(x => x.IniciadaEm)
           .HasColumnName("INICIADA_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Property(x => x.ConcluidaEm)
           .HasColumnName("CONCLUIDA_EM")
           .HasColumnType("timestamp with time zone");

    builder.Property(x => x.ExpiraEm)
           .HasColumnName("EXPIRA_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Property(x => x.QuantidadeFontesConsultadas)
           .HasColumnName("QUANTIDADE_FONTES_CONSULTADAS")
           .HasColumnType("integer")
           .IsRequired();

    builder.Property(x => x.QuantidadeFontesComSucesso)
           .HasColumnName("QUANTIDADE_FONTES_COM_SUCESSO")
           .HasColumnType("integer")
           .IsRequired();

    builder.Property(x => x.QuantidadeFontesComFalha)
           .HasColumnName("QUANTIDADE_FONTES_COM_FALHA")
           .HasColumnType("integer")
           .IsRequired();

    builder.Navigation(x => x.Etapas)
           .HasField("_etapas")
           .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.HasIndex(x => new { x.ContaId, x.ChaveIdempotencia })
           .IsUnique()
           .HasFilter("\"CONTA_ID\" IS NOT NULL");

    builder.HasIndex(x => new { x.VisitanteId, x.ChaveIdempotencia })
           .IsUnique()
           .HasFilter("\"VISITANTE_ID\" IS NOT NULL");
  }
}

