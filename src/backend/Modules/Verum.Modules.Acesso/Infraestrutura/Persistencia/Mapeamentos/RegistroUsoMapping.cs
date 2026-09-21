using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Acesso.Dominio.RegistroUso;

namespace Verum.Modules.Acesso.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class RegistroUsoMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("registro_uso", table =>
    {
      table.HasCheckConstraint("ck_registro_uso_conta_id", "conta_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_registro_uso_visitante_id", "visitante_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_registro_uso_recurso_codigo", "length(btrim(recurso_codigo)) > 0");
      table.HasCheckConstraint("ck_registro_uso_quantidade", "quantidade >= 1 AND quantidade <= 2147483647");
      table.HasCheckConstraint("ck_registro_uso_referencia_id", "referencia_id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_registro_uso_dados", "jsonb_typeof(dados) = 'object'");
      table.HasCheckConstraint("ck_registro_uso_regra_6", "(conta_id IS NULL) <> (visitante_id IS NULL)");
    });
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").IsRequired().UseIdentityByDefaultColumn();
    builder.Property(x => x.ContaId).HasColumnName("conta_id").HasColumnType("uuid");
    builder.Property(x => x.VisitanteId).HasColumnName("visitante_id").HasColumnType("uuid");
    builder.Property(x => x.RecursoCodigo).HasColumnName("recurso_codigo").HasMaxLength(100).HasColumnType("varchar(100)").IsRequired();
    builder.Property(x => x.Quantidade).HasColumnName("quantidade").HasColumnType("integer").IsRequired();
    builder.Property(x => x.ReferenciaId).HasColumnName("referencia_id").HasColumnType("uuid");
    builder.Property(x => x.Dados).HasColumnName("dados").HasColumnType("jsonb");
    builder.Property(x => x.OcorridoEm).HasColumnName("ocorrido_em").HasColumnType("timestamp with time zone").IsRequired();
  }
}

