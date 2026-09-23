using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Assinaturas.Dominio.Pagamento;

namespace Verum.Modules.Assinaturas.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class PagamentoMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("PAGAMENTO", table =>
    {
      table.HasCheckConstraint("CK_PAGAMENTO_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_PAGAMENTO_ASSINATURA_ID", "\"ASSINATURA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

      table.HasCheckConstraint("CK_PAGAMENTO_IDENTIFICADOR_EXTERNO", "length(btrim(\"IDENTIFICADOR_EXTERNO\")) > 0");

      table.HasCheckConstraint("CK_PAGAMENTO_STATUS", "\"STATUS\" IN (1, 2, 3, 4, 5)");

      table.HasCheckConstraint("CK_PAGAMENTO_VALOR", "\"VALOR\" >= 0.01 AND \"VALOR\" <= 999999999999.99");

      table.HasCheckConstraint("CK_PAGAMENTO_MOEDA", "length(btrim(\"MOEDA\")) > 0");

      table.HasCheckConstraint("CK_PAGAMENTO_METODO", "\"METODO\" IN (1, 2, 3)");
    });

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
           .HasColumnName("ID")
           .HasColumnType("uuid")
           .IsRequired()
           .ValueGeneratedNever();

    builder.Property(x => x.AssinaturaId)
           .HasColumnName("ASSINATURA_ID")
           .HasColumnType("uuid")
           .IsRequired();

    builder.Property(x => x.IdentificadorExterno)
           .HasColumnName("IDENTIFICADOR_EXTERNO")
           .HasMaxLength(200)
           .HasColumnType("varchar(200)")
           .IsRequired();

    builder.Property(x => x.Status)
           .HasColumnName("STATUS")
           .HasConversion<short>()
           .HasColumnType("smallint")
           .IsRequired();

    builder.Property(x => x.Valor)
           .HasColumnName("VALOR")
           .HasPrecision(14, 2)
           .HasColumnType("numeric(14,2)")
           .IsRequired();

    builder.Property(x => x.Moeda)
           .HasColumnName("MOEDA")
           .HasMaxLength(3)
           .IsFixedLength()
           .HasColumnType("char(3)")
           .IsRequired();

    builder.Property(x => x.Metodo)
           .HasColumnName("METODO")
           .HasConversion<short>()
           .HasColumnType("smallint");

    builder.Property(x => x.VencimentoEm)
           .HasColumnName("VENCIMENTO_EM")
           .HasColumnType("timestamp with time zone");

    builder.Property(x => x.PagoEm)
           .HasColumnName("PAGO_EM")
           .HasColumnType("timestamp with time zone");

    builder.Property(x => x.CriadoEm)
           .HasColumnName("CRIADO_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.Property(x => x.AtualizadoEm)
           .HasColumnName("ATUALIZADO_EM")
           .HasColumnType("timestamp with time zone")
           .IsRequired();

    builder.HasOne(x => x.Assinatura)
           .WithMany(x => x.Pagamentos)
           .HasForeignKey(x => x.AssinaturaId)
           .OnDelete(DeleteBehavior.Restrict);

    builder.HasIndex(x => new { x.AssinaturaId, x.IdentificadorExterno })
           .IsUnique();
  }
}

