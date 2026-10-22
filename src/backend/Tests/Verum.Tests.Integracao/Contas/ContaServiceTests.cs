using Microsoft.EntityFrameworkCore;
using Npgsql;
using Verum.BuildingBlocks.Erros;
using Verum.Modules.Contas.Aplicacao;
using Verum.Modules.Contas.Dominio;
using Verum.Modules.Contas.Infraestrutura.Persistencia;
using Xunit;

namespace Verum.Tests.Integracao;

public sealed class ContaServiceTests
{
  [PostgreSqlFact]
  public async Task SincronizacaoConcorrenteMantemUmaContaAtualizaPerfilERejeitaContaBloqueada()
  {
    var configuracao = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("VERUM_TEST_POSTGRES"));

    var banco = "verum_contas_test_" + Guid.NewGuid().ToString("N");

    await using var admin = new NpgsqlConnection(configuracao.ConnectionString);

    await admin.OpenAsync();

    await using (var criar = new NpgsqlCommand($"CREATE DATABASE \"{banco}\"", admin))
      await criar.ExecuteNonQueryAsync();

    configuracao.Database = banco;

    var options = new DbContextOptionsBuilder<ContasDbContext>().UseNpgsql(configuracao.ConnectionString).Options;

    try
    {
      await using (var db = new ContasDbContext(options))
        await db.Database.MigrateAsync();

      var sujeito = Guid.NewGuid().ToString();

      async Task<Guid> Sincronizar()
      {
        await using var db = new ContasDbContext(options);

        return (await new ContaService(db).SincronizarAsync(sujeito, "teste@example.test", "Teste", default)).Id;
      }

      var resultados = await Task.WhenAll(Enumerable.Range(0, 6).Select(_ => Sincronizar()));

      Assert.Single(resultados.Distinct());

      await using var verificar = new ContasDbContext(options);

      Assert.Equal(1, await verificar.Contas.CountAsync());

      var service = new ContaService(verificar);

      var atualizada = await service.SincronizarAsync(sujeito, "novo@example.test", "Novo nome", default);

      Assert.Equal(resultados[0], atualizada.Id);

      Assert.Equal("novo@example.test", atualizada.Email);

      Assert.Equal("Novo nome", atualizada.NomeExibicao);

      verificar.ChangeTracker.Clear();

      var consultada = await service.ObterAsync(sujeito, default);

      Assert.Equal(atualizada, consultada);

      Assert.Empty(verificar.ChangeTracker.Entries());

      var ausente = await Assert.ThrowsAsync<ErroAplicacaoException>(() => service.ObterAsync("outra-identidade", default));

      Assert.Equal(TipoErro.NaoEncontrado, ausente.Tipo);

      var conta = await verificar.Contas.SingleAsync();

      verificar.Entry(conta).Property(x => x.Status).CurrentValue = StatusConta.Bloqueada;

      await verificar.SaveChangesAsync();

      var consultaBloqueada = await Assert.ThrowsAsync<ErroAplicacaoException>(() => service.ObterAsync(sujeito, default));

      Assert.Equal(TipoErro.AcessoNegado, consultaBloqueada.Tipo);

      var erro = await Assert.ThrowsAsync<ErroAplicacaoException>(() =>
        service.SincronizarAsync(sujeito, "novo@example.test", "Novo nome", default));

      Assert.Equal(TipoErro.AcessoNegado, erro.Tipo);
    }
    finally
    {
      // Nome exclusivo criado neste teste; nenhum banco existente é removido.
      NpgsqlConnection.ClearAllPools();

      await using var remover = new NpgsqlCommand($"DROP DATABASE \"{banco}\" WITH (FORCE)", admin);

      await remover.ExecuteNonQueryAsync();
    }
  }
}
