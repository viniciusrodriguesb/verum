using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Ofertas.Dominio.ExecucaoConsultaFonte;

namespace Verum.Modules.Ofertas.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class ExecucaoConsultaFonteMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("execucao_consulta_fonte", table =>
    {
      table.HasCheckConstraint("ck_execucao_consulta_fonte_id", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_execucao_consulta_fonte_correlacao_id", "correlacao_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_execucao_consulta_fonte_busca_id", "busca_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_execucao_consulta_fonte_fonte_oferta_id", "fonte_oferta_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_execucao_consulta_fonte_tipo_provedor", "tipo_provedor IN (1, 2, 3, 4, 5)");
      table.HasCheckConstraint("ck_execucao_consulta_fonte_status", "status IN (1, 2, 3, 4, 5, 6, 7)");
      table.HasCheckConstraint("ck_execucao_consulta_fonte_quantidade_itens_encontrados", "quantidade_itens_encontrados >= 0 AND quantidade_itens_encontrados <= 2147483647");
      table.HasCheckConstraint("ck_execucao_consulta_fonte_quantidade_itens_aceitos", "quantidade_itens_aceitos >= 0 AND quantidade_itens_aceitos <= 2147483647");
      table.HasCheckConstraint("ck_execucao_consulta_fonte_numero_tentativas", "numero_tentativas >= 0 AND numero_tentativas <= 32767");
      table.HasCheckConstraint("ck_execucao_consulta_fonte_codigo_erro", "length(btrim(codigo_erro)) > 0");
      table.HasCheckConstraint("ck_execucao_consulta_fonte_detalhes_erro", "length(btrim(detalhes_erro)) > 0");
    });

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
           .HasColumnName("id")
           .HasColumnType("uuid")
           .IsRequired()
           .ValueGeneratedNever();

    builder.Property(x => x.CorrelacaoId)
           .HasColumnName("correlacao_id")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.BuscaId)
           .HasColumnName("busca_id")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.FonteOfertaId)
           .HasColumnName("fonte_oferta_id")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.TipoProvedor)
           .HasColumnName("tipo_provedor")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.Status)
           .HasColumnName("status")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.QuantidadeItensEncontrados)
           .HasColumnName("quantidade_itens_encontrados")
           .HasColumnType("integer")
           .IsRequired();

    builder.Property(x => x.QuantidadeItensAceitos)
           .HasColumnName("quantidade_itens_aceitos")
           .HasColumnType("integer")
           .IsRequired();

    builder.Property(x => x.IniciadaEm)
           .HasColumnName("iniciada_em")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Property(x => x.FinalizadaEm)
           .HasColumnName("finalizada_em")
           .HasColumnType("timestamp with time zone");

    builder.Property(x => x.NumeroTentativas)
           .HasColumnName("numero_tentativas")
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.CodigoErro)
           .HasColumnName("codigo_erro")
           .HasMaxLength(100)
           .HasColumnType("varchar(100)");

    builder.Property(x => x.DetalhesErro)
           .HasColumnName("detalhes_erro")
           .HasMaxLength(1000)
           .HasColumnType("varchar(1000)");

    builder.HasOne(x => x.Fonte)
           .WithMany(x => x.Execucoes)
           .HasForeignKey(x => x.FonteOfertaId)
           .OnDelete(DeleteBehavior.Restrict);
  }
}

