using Xunit;

namespace Verum.Tests.Integracao.Carga.Configuracoes;

public sealed class CargaLocalFactAttribute : FactAttribute
{
  public CargaLocalFactAttribute()
  {
    if (Environment.GetEnvironmentVariable("VERUM_TEST_CARGA_REAL") != "1"
      || Environment.GetEnvironmentVariable("VERUM_TEST_RABBITMQ") != "1"
      || string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("VERUM_TEST_POSTGRES")))
      Skip = "Carga optativa: configure VERUM_TEST_CARGA_REAL, VERUM_TEST_RABBITMQ e VERUM_TEST_POSTGRES.";
  }
}
