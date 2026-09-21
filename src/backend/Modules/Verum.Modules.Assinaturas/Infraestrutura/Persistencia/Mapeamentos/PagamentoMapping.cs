using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Assinaturas.Dominio.Pagamento;

namespace Verum.Modules.Assinaturas.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class PagamentoMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("pagamento", table =>
    {
      table.HasCheckConstraint("ck_pagamento_id", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_pagamento_assinatura_id", "assinatura_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_pagamento_identificador_externo", "length(btrim(identificador_externo)) > 0");
      table.HasCheckConstraint("ck_pagamento_status", "status IN (1, 2, 3, 4, 5)");
      table.HasCheckConstraint("ck_pagamento_valor", "valor >= 0.01 AND valor <= 999999999999.99");
      table.HasCheckConstraint("ck_pagamento_moeda", "length(btrim(moeda)) > 0");
      table.HasCheckConstraint("ck_pagamento_metodo", "metodo IN (1, 2, 3)");
    });
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
    builder.Property(x => x.AssinaturaId).HasColumnName("assinatura_id").HasColumnType("uuid").IsRequired();
    builder.Property(x => x.IdentificadorExterno).HasColumnName("identificador_externo").HasMaxLength(200).HasColumnType("varchar(200)").IsRequired();
    builder.Property(x => x.Status).HasColumnName("status").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.Valor).HasColumnName("valor").HasPrecision(14, 2).HasColumnType("numeric(14,2)").IsRequired();
    builder.Property(x => x.Moeda).HasColumnName("moeda").HasMaxLength(3).IsFixedLength().HasColumnType("char(3)").IsRequired();
    builder.Property(x => x.Metodo).HasColumnName("metodo").HasConversion<short>().HasColumnType("smallint");
    builder.Property(x => x.VencimentoEm).HasColumnName("vencimento_em").HasColumnType("timestamp with time zone");
    builder.Property(x => x.PagoEm).HasColumnName("pago_em").HasColumnType("timestamp with time zone");
    builder.Property(x => x.CriadoEm).HasColumnName("criado_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Property(x => x.AtualizadoEm).HasColumnName("atualizado_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.HasOne(x => x.Assinatura).WithMany(x => x.Pagamentos).HasForeignKey(x => x.AssinaturaId).OnDelete(DeleteBehavior.Restrict);
    builder.HasIndex(x => new { x.AssinaturaId, x.IdentificadorExterno }).IsUnique();
  }
}

