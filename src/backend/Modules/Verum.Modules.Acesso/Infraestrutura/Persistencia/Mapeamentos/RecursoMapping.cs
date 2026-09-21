using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entidade = Verum.Modules.Acesso.Dominio.Recurso;

namespace Verum.Modules.Acesso.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class RecursoMapping : IEntityTypeConfiguration<Entidade>
{
  public void Configure(EntityTypeBuilder<Entidade> builder)
  {
    builder.ToTable("recurso", table =>
    {
      table.HasCheckConstraint("ck_recurso_id", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
      table.HasCheckConstraint("ck_recurso_codigo", "length(btrim(codigo)) > 0");
      table.HasCheckConstraint("ck_recurso_nome", "length(btrim(nome)) > 0");
      table.HasCheckConstraint("ck_recurso_descricao", "length(btrim(descricao)) > 0");
      table.HasCheckConstraint("ck_recurso_tipo_limite", "tipo_limite IN (1, 2)");
    });
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
    builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(100).HasColumnType("varchar(100)").IsRequired();
    builder.Property(x => x.Nome).HasColumnName("nome").HasMaxLength(150).HasColumnType("varchar(150)").IsRequired();
    builder.Property(x => x.Descricao).HasColumnName("descricao").HasMaxLength(500).HasColumnType("varchar(500)").IsRequired();
    builder.Property(x => x.TipoLimite).HasColumnName("tipo_limite").HasConversion<short>().HasColumnType("smallint").IsRequired();
    builder.Property(x => x.Ativo).HasColumnName("ativo").HasColumnType("boolean").IsRequired();
    builder.Property(x => x.CriadoEm).HasColumnName("criado_em").HasColumnType("timestamp with time zone").IsRequired();
    builder.Navigation(x => x.Pacotes).HasField("_pacotes").UsePropertyAccessMode(PropertyAccessMode.Field);
    builder.HasIndex(x => x.Codigo).IsUnique();
  }
}

