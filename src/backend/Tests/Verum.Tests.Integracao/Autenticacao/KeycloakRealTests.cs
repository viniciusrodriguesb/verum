using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Verum.CrossCutting.Pipelines;
using Xunit;

namespace Verum.Tests.Integracao;

public sealed class KeycloakTokenFactAttribute : FactAttribute
{
  public KeycloakTokenFactAttribute()
  {
    if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("VERUM_TEST_KEYCLOAK_ACCESS_TOKEN"))
      || string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("VERUM_TEST_KEYCLOAK_ID_TOKEN")))
      Skip = "Informe tokens de acesso e identidade obtidos com Authorization Code e PKCE no realm local.";
  }
}

public sealed class KeycloakRealTests
{
  [KeycloakTokenFact]
  public async Task PipelineAceitaTokenRealDeAcessoERejeitaIdTokenESemToken()
  {
    var builder = WebApplication.CreateBuilder();

    builder.Logging.ClearProviders();

    builder.WebHost.UseTestServer();

    builder.Services.AddVerumAutenticacao(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
      ["Authentication:Authority"] = Environment.GetEnvironmentVariable("VERUM_TEST_KEYCLOAK_AUTHORITY")
        ?? "http://localhost:8081/realms/verum",

      ["Authentication:Audience"] = "verum-api",

      ["Authentication:RequireHttpsMetadata"] = "false"
    }).Build());

    await using var app = builder.Build();

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapGet("/protegido", (System.Security.Claims.ClaimsPrincipal usuario) =>
      usuario.FindFirst("sub")?.Value ?? string.Empty).RequireAuthorization();

    await app.StartAsync();

    using var client = app.GetTestClient();

    Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/protegido")).StatusCode);

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
      Environment.GetEnvironmentVariable("VERUM_TEST_KEYCLOAK_ID_TOKEN"));

    Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/protegido")).StatusCode);

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
      Environment.GetEnvironmentVariable("VERUM_TEST_KEYCLOAK_ACCESS_TOKEN"));

    using var autenticada = await client.GetAsync("/protegido");

    Assert.Equal(HttpStatusCode.OK, autenticada.StatusCode);

    Assert.True(Guid.TryParse(await autenticada.Content.ReadAsStringAsync(), out _));
  }
}
