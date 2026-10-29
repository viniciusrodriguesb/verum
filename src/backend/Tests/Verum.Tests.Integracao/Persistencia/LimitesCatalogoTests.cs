using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Verum.BuildingBlocks.Erros;
using Verum.Modules.Catalogo.Contratos;
using Verum.Modules.Catalogo.Infraestrutura.Persistencia;
using Xunit;

namespace Verum.Tests.Integracao;

public sealed class LimitesCatalogoTests
{
  [PostgreSqlFact]
  public async Task CatalogoPersisteNomeDe300EConsultaDe500SemTruncarERejeitaExcesso()
  {
    await using var ambiente = await FluxoBuscaPersistenciaTests.Ambiente.CriarAsync();

    await using var scope = ambiente.Host.Services.CreateAsyncScope();

    var catalogo = scope.ServiceProvider.GetRequiredService<ICatalogoDescobertaService>();

    var nome = "Capacete bicicleta ".PadRight(300, 'a');

    var consulta = "capacete bicicleta ".PadRight(500, 'b');

    var id = await catalogo.RegistrarAsync(consulta, nome, "https://loja.example/capacete", default);

    var db = scope.ServiceProvider.GetRequiredService<CatalogoDbContext>();

    Assert.Equal(nome, (await db.Produtos.AsNoTracking().SingleAsync()).Nome);

    Assert.Equal(consulta, (await db.TermosBusca.AsNoTracking().SingleAsync()).TermoOriginal);

    Assert.Equal(id, Assert.Single(await catalogo.ResolverAsync(consulta, default)));

    await Assert.ThrowsAsync<ErroAplicacaoException>(() => catalogo.RegistrarAsync(consulta,
      nome + "a", "https://loja.example/excesso", default));

    Assert.Equal(1, await db.Produtos.CountAsync());
  }
}
