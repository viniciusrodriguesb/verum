extern alias VerumApi;

using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Verum.Modules.Acesso.Contratos.DTOs;
using Verum.Modules.Busca.Contratos.DTOs;
using Verum.Modules.Contas.Infraestrutura.Persistencia;
using Xunit;

namespace Verum.Tests.Integracao;

public sealed class AcessoBuscaApiTests
{
  [PostgreSqlFact]
  public async Task ApiSeparaVisitanteContaNavegadoresEBloqueioSemConfiarNoCorpo()
  {
    await using var ambiente = await FluxoBuscaPersistenciaTests.Ambiente.CriarAsync(limite: 1);

    await using var factory = ambiente.CriarApi().WithWebHostBuilder(builder => builder.ConfigureServices(services =>
      services.AddAuthentication("IdentidadeTeste").AddScheme<AuthenticationSchemeOptions, IdentidadeTesteHandler>("IdentidadeTeste", _ => { })));

    using var navegador = factory.CreateClient();

    var visitante = await navegador.GetFromJsonAsync<AcessoBusca>("/api/v1/buscas/acesso");

    Assert.Equal("Visitante", visitante!.Contexto);

    using var pedidoVisitante = await navegador.PostAsJsonAsync("/api/v1/buscas", new SolicitarBusca("ssd kingston", "chave"));

    Assert.Equal(HttpStatusCode.Accepted, pedidoVisitante.StatusCode);

    var idVisitante = (await pedidoVisitante.Content.ReadFromJsonAsync<BuscaAceita>())!.Id;

    var sujeito = Guid.NewGuid().ToString();

    navegador.DefaultRequestHeaders.Add("X-Identidade-Teste", sujeito);

    var free = await navegador.GetFromJsonAsync<AcessoBusca>("/api/v1/buscas/acesso");

    Assert.Equal("Free", free!.Contexto);

    Assert.Equal(3, free.Restante);

    Assert.Empty((await navegador.GetFromJsonAsync<BuscaHistorico[]>("/api/v1/buscas/historico"))!);

    Assert.Equal(HttpStatusCode.NotFound, (await navegador.GetAsync($"/api/v1/buscas/{idVisitante}")).StatusCode);

    using var pedidoConta = await navegador.PostAsJsonAsync("/api/v1/buscas", new SolicitarBusca("ssd kingston", "chave"));

    Assert.Equal(HttpStatusCode.Accepted, pedidoConta.StatusCode);

    var idConta = (await pedidoConta.Content.ReadFromJsonAsync<BuscaAceita>())!.Id;

    using var outroNavegador = factory.CreateClient();

    outroNavegador.DefaultRequestHeaders.Add("X-Identidade-Teste", sujeito);

    Assert.Equal(HttpStatusCode.OK, (await outroNavegador.GetAsync($"/api/v1/buscas/{idConta}")).StatusCode);

    Assert.Equal(idConta, (await outroNavegador.GetFromJsonAsync<BuscaHistorico[]>("/api/v1/buscas/historico"))!.Single().Id);

    outroNavegador.DefaultRequestHeaders.Remove("X-Identidade-Teste");

    outroNavegador.DefaultRequestHeaders.Add("X-Identidade-Teste", Guid.NewGuid().ToString());

    Assert.Equal(HttpStatusCode.NotFound, (await outroNavegador.GetAsync($"/api/v1/buscas/{idConta}")).StatusCode);

    navegador.DefaultRequestHeaders.Remove("X-Identidade-Teste");

    Assert.Equal(HttpStatusCode.NotFound, (await navegador.GetAsync($"/api/v1/buscas/{idConta}")).StatusCode);

    Assert.Equal(HttpStatusCode.Unauthorized, (await navegador.GetAsync("/api/v1/buscas/historico")).StatusCode);

    navegador.DefaultRequestHeaders.Add("Authorization", "Bearer invalido");

    Assert.Equal(HttpStatusCode.Unauthorized, (await navegador.GetAsync("/api/v1/buscas/acesso")).StatusCode);

    navegador.DefaultRequestHeaders.Remove("Authorization");

    navegador.DefaultRequestHeaders.Add("X-Identidade-Teste", sujeito);

    await using (var scope = factory.Services.CreateAsyncScope())
    {
      var db = scope.ServiceProvider.GetRequiredService<ContasDbContext>();

      var conta = await db.Contas.SingleAsync(x => x.SujeitoIdentidade == sujeito);

      conta.Bloquear();

      await db.SaveChangesAsync();
    }

    Assert.Equal(HttpStatusCode.Forbidden, (await navegador.GetAsync("/api/v1/buscas/acesso")).StatusCode);

    Assert.Equal(HttpStatusCode.Forbidden, (await navegador.GetAsync($"/api/v1/buscas/{idConta}")).StatusCode);

    Assert.Equal(HttpStatusCode.Forbidden, (await navegador.PostAsJsonAsync("/api/v1/buscas", new SolicitarBusca("outro produto", "bloqueada"))).StatusCode);
  }

  public sealed class IdentidadeTesteHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
  {
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
      var sujeito = Request.Headers["X-Identidade-Teste"].FirstOrDefault();

      if (sujeito is null) return Task.FromResult(AuthenticateResult.NoResult());

      var identidade = new ClaimsIdentity([
        new Claim("sub", sujeito),

        new Claim("email", "conta-" + sujeito + "@example.test"),

        new Claim("email_verified", "true"),

        new Claim("name", "Conta de teste")
      ], Scheme.Name);

      return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identidade), Scheme.Name)));
    }
  }
}
