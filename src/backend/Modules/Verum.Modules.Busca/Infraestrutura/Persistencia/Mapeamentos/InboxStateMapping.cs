using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.RegularExpressions;

namespace Verum.Modules.Busca.Infraestrutura.Persistencia.Mapeamentos;

internal sealed class InboxStateMapping : IEntityTypeConfiguration<InboxState>
{
  public void Configure(EntityTypeBuilder<InboxState> builder)
  {
    builder.ConfigureInboxStateEntity();

    builder.ToTable("INBOX_STATE");

    // Tipos técnicos da biblioteca seguem os mesmos nomes SQL do backend.
    foreach (var propriedade in builder.Metadata.GetProperties())
      propriedade.SetColumnName(Regex.Replace(propriedade.Name, "([a-z])([A-Z])", "$1_$2").ToUpperInvariant());
  }
}
