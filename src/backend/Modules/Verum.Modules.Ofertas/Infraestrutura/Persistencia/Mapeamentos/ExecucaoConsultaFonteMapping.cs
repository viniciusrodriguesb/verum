using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Ofertas.Dominio.ExecucaoConsultaFonte;

namespace Verum.Modules.Ofertas.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class ExecucaoConsultaFonteMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("EXECUCAO_CONSULTA_FONTE", table =>
    {
      table.HasCheckConstraint("CK_EXECUCAO_CONSULTA_FONTE_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_EXECUCAO_CONSULTA_FONTE_CORRELACAO_ID", "\"CORRELACAO_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_EXECUCAO_CONSULTA_FONTE_BUSCA_ID", "\"BUSCA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_EXECUCAO_CONSULTA_FONTE_FONTE_OFERTA_ID", "\"FONTE_OFERTA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_EXECUCAO_CONSULTA_FONTE_TIPO_PROVEDOR", "\"TIPO_PROVEDOR\" IN (1, 2, 3, 4, 5)");

      table.HasCheckConstraint("CK_EXECUCAO_CONSULTA_FONTE_STATUS", "\"STATUS\" IN (1, 2, 3, 4, 5, 6, 7)");

      table.HasCheckConstraint("CK_EXECUCAO_CONSULTA_FONTE_QUANTIDADE_ITENS_ENCONTRADOS", "\"QUANTIDADE_ITENS_ENCONTRADOS\" >= 0 AND \"QUANTIDADE_ITENS_ENCONTRADOS\" <= 2147483647");

      table.HasCheckConstraint("CK_EXECUCAO_CONSULTA_FONTE_QUANTIDADE_ITENS_ACEITOS", "\"QUANTIDADE_ITENS_ACEITOS\" >= 0 AND \"QUANTIDADE_ITENS_ACEITOS\" <= 2147483647");

      table.HasCheckConstraint("CK_EXECUCAO_CONSULTA_FONTE_NUMERO_TENTATIVAS", "\"NUMERO_TENTATIVAS\" >= 0 AND \"NUMERO_TENTATIVAS\" <= 32767");

      table.HasCheckConstraint("CK_EXECUCAO_CONSULTA_FONTE_CODIGO_ERRO", "length(btrim(\"CODIGO_ERRO\")) > 0");

      table.HasCheckConstraint("CK_EXECUCAO_CONSULTA_FONTE_DETALHES_ERRO", "length(btrim(\"DETALHES_ERRO\")) > 0");
    });

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
           .HasColumnName("ID")
           .HasColumnType("uuid")
           .IsRequired()
           .ValueGeneratedNever();

    builder.Property(x => x.CorrelacaoId)
           .HasColumnName("CORRELACAO_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.BuscaId)
           .HasColumnName("BUSCA_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.FonteOfertaId)
           .HasColumnName("FONTE_OFERTA_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.TipoProvedor)
           .HasColumnName("TIPO_PROVEDOR")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.Status)
           .HasColumnName("STATUS")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.QuantidadeItensEncontrados)
           .HasColumnName("QUANTIDADE_ITENS_ENCONTRADOS")
           .HasColumnType("integer")
           .IsRequired();

    builder.Property(x => x.QuantidadeItensAceitos)
           .HasColumnName("QUANTIDADE_ITENS_ACEITOS")
           .HasColumnType("integer")
           .IsRequired();

    builder.Property(x => x.IniciadaEm)
           .HasColumnName("INICIADA_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Property(x => x.FinalizadaEm)
           .HasColumnName("FINALIZADA_EM")
           .HasColumnType("timestamp with time zone");

    builder.Property(x => x.NumeroTentativas)
           .HasColumnName("NUMERO_TENTATIVAS")
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.CodigoErro)
           .HasColumnName("CODIGO_ERRO")
           .HasMaxLength(100)
           .HasColumnType("varchar(100)");

    builder.Property(x => x.DetalhesErro)
           .HasColumnName("DETALHES_ERRO")
           .HasMaxLength(1000)
           .HasColumnType("varchar(1000)");

    builder.HasOne(x => x.Fonte)
           .WithMany(x => x.Execucoes)
           .HasForeignKey(x => x.FonteOfertaId)
           .OnDelete(DeleteBehavior.Restrict);
  }
}

