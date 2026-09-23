using Xunit;

namespace Verum.Tests.Integracao;

public sealed class PostgreSqlFactAttribute : FactAttribute
{
  public PostgreSqlFactAttribute()
  {
    if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("VERUM_TEST_POSTGRES")))
      Skip = "Configure VERUM_TEST_POSTGRES para validar o DDL no PostgreSQL real.";
  }
}
