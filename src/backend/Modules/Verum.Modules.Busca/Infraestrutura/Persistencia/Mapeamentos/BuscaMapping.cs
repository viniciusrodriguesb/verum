using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Busca.Dominio.Busca;

namespace Verum.Modules.Busca.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class BuscaMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("busca", table =>
    {
      table.HasCheckConstraint("ck_busca_id", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_busca_conta_id", "conta_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_busca_visitante_id", "visitante_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_busca_chave_idempotencia", "length(btrim(chave_idempotencia)) > 0");
      table.HasCheckConstraint("ck_busca_consulta_original", "length(btrim(consulta_original)) > 0");
      table.HasCheckConstraint("ck_busca_consulta_normalizada", "length(btrim(consulta_normalizada)) > 0");
      table.HasCheckConstraint("ck_busca_consulta_estruturada", "jsonb_typeof(consulta_estruturada) = 'object'");
      table.HasCheckConstraint("ck_busca_produto_principal_id", "produto_principal_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_busca_produto_variante_principal_id", "produto_variante_principal_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_busca_status", "status IN (1, 2, 3, 4, 5, 6)");
      table.HasCheckConstraint("ck_busca_etapa_atual", "etapa_atual IN (1, 2, 3, 4, 5, 6)");
      table.HasCheckConstraint("ck_busca_motivo_sem_resultado", "motivo_sem_resultado IN (1, 2, 3, 4)");
      table.HasCheckConstraint("ck_busca_codigo_erro", "length(btrim(codigo_erro)) > 0");
      table.HasCheckConstraint("ck_busca_mensagem_erro", "length(btrim(mensagem_erro)) > 0");
      table.HasCheckConstraint("ck_busca_quantidade_fontes_consultadas", "quantidade_fontes_consultadas >= 0 AND quantidade_fontes_consultadas <= 2147483647");
      table.HasCheckConstraint("ck_busca_quantidade_fontes_com_sucesso", "quantidade_fontes_com_sucesso >= 0 AND quantidade_fontes_com_sucesso <= 2147483647");
      table.HasCheckConstraint("ck_busca_quantidade_fontes_com_falha", "quantidade_fontes_com_falha >= 0 AND quantidade_fontes_com_falha <= 2147483647");
      table.HasCheckConstraint("ck_busca_regra_17", "(conta_id IS NULL) <> (visitante_id IS NULL)");
      table.HasCheckConstraint("ck_busca_regra_18", "expira_em > iniciada_em");
    });
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
    builder.Property(x => x.ContaId).HasColumnName("conta_id").HasColumnType("uuid");
    builder.Property(x => x.VisitanteId).HasColumnName("visitante_id").HasColumnType("uuid");
    builder.Property(x => x.ChaveIdempotencia).HasColumnName("chave_idempotencia").HasMaxLength(100).HasColumnType("varchar(100)").IsRequired();
    builder.Property(x => x.ConsultaOriginal).HasColumnName("consulta_original").HasMaxLength(500).HasColumnType("varchar(500)").IsRequired();
    builder.Property(x => x.ConsultaNormalizada).HasColumnName("consulta_normalizada").HasMaxLength(500).HasColumnType("varchar(500)").IsRequired();
    builder.Property(x => x.ConsultaEstruturada).HasColumnName("consulta_estruturada").HasColumnType("jsonb");
    builder.Property(x => x.ProdutoPrincipalId).HasColumnName("produto_principal_id").HasColumnType("uuid");
    builder.Property(x => x.ProdutoVariantePrincipalId).HasColumnName("produto_variante_principal_id").HasColumnType("uuid");
    builder.Property(x => x.Status).HasColumnName("status").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.EtapaAtual).HasColumnName("etapa_atual").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.MotivoSemResultado).HasColumnName("motivo_sem_resultado").HasConversion<short>().HasColumnType("smallint");
    builder.Property(x => x.ResultadoParcial).HasColumnName("resultado_parcial").HasColumnType("boolean").IsRequired();
    builder.Property(x => x.CodigoErro).HasColumnName("codigo_erro").HasMaxLength(100).HasColumnType("varchar(100)");
    builder.Property(x => x.MensagemErro).HasColumnName("mensagem_erro").HasMaxLength(500).HasColumnType("varchar(500)");
    builder.Property(x => x.IniciadaEm).HasColumnName("iniciada_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Property(x => x.ConcluidaEm).HasColumnName("concluida_em").HasColumnType("timestamp with time zone");
    builder.Property(x => x.ExpiraEm).HasColumnName("expira_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Property(x => x.QuantidadeFontesConsultadas).HasColumnName("quantidade_fontes_consultadas").HasColumnType("integer").IsRequired();
    builder.Property(x => x.QuantidadeFontesComSucesso).HasColumnName("quantidade_fontes_com_sucesso").HasColumnType("integer").IsRequired();
    builder.Property(x => x.QuantidadeFontesComFalha).HasColumnName("quantidade_fontes_com_falha").HasColumnType("integer").IsRequired();
    builder.Navigation(x => x.Etapas).HasField("_etapas").UsePropertyAccessMode(PropertyAccessMode.Field);
    builder.HasIndex(x => new { x.ContaId, x.ChaveIdempotencia }).IsUnique().HasFilter("conta_id IS NOT NULL");
    builder.HasIndex(x => new { x.VisitanteId, x.ChaveIdempotencia }).IsUnique().HasFilter("visitante_id IS NOT NULL");
  }
}

