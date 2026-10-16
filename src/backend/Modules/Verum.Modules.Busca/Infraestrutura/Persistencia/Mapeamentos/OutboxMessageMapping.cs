using System.Text.RegularExpressions;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Verum.Modules.Busca.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class OutboxMessageMapping : IEntityTypeConfiguration<OutboxMessage>
{
  public void Configure(EntityTypeBuilder<OutboxMessage> builder)
  {
    builder.ConfigureOutboxMessageEntity();

    builder.ToTable("OUTBOX_MESSAGE");

    // Tipos técnicos da biblioteca seguem os mesmos nomes SQL do backend.
    foreach (var propriedade in builder.Metadata.GetProperties())
      propriedade.SetColumnName(Regex.Replace(propriedade.Name, "([a-z])([A-Z])", "$1_$2").ToUpperInvariant());
  }
}
