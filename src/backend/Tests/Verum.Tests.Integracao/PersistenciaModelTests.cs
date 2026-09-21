using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Verum.Tests.Integracao;

public sealed class PersistenciaModelTests
{
  private static readonly (string Name, int Count)[] Modulos =
  [
    ("Contas", 1), ("Catalogo", 7), ("Ofertas", 5), ("Busca", 4),
    ("Radar", 2), ("Notificacoes", 3), ("Acesso", 5), ("Assinaturas", 5)
  ];

  public static IEnumerable<object[]> Contextos() => Modulos.Select(m => new object[] { m.Name, m.Count });

  [Theory]
  [MemberData(nameof(Contextos))]
  public void CadaContextoAplicaMappingsEProduzSqlPostgreSqlSemAcessarBanco(string modulo, int quantidade)
  {
    using var provider = Provider(modulo);
    using var scope = provider.CreateScope();
    using var context = Context(scope.ServiceProvider, modulo);
    var entities = context.Model.GetEntityTypes().ToArray();
    Assert.Equal(quantidade, entities.Length);
    Assert.Equal(modulo.ToLowerInvariant(), context.Model.GetDefaultSchema());
    foreach (var entity in entities)
    {
      Assert.Equal(context.GetType().Assembly, entity.ClrType.Assembly);
      Assert.Equal(modulo.ToLowerInvariant(), entity.GetSchema());
      Assert.NotNull(entity.FindPrimaryKey());
      Assert.All(entity.GetProperties(), property =>
      {
        Assert.False(property.IsShadowProperty(), $"{entity.Name}.{property.Name} é shadow property.");
        Assert.NotNull(property.GetColumnType());
        Assert.True(property.PropertyInfo!.SetMethod!.IsPrivate);
      });
      Assert.All(entity.GetForeignKeys(), fk =>
      {
        Assert.Equal(entity.GetSchema(), fk.PrincipalEntityType.GetSchema());
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
      });
      Assert.All(entity.GetNavigations().Where(n => n.IsCollection), navigation =>
      {
        Assert.NotNull(navigation.FieldInfo);
        Assert.Null(navigation.PropertyInfo!.SetMethod);
      });
    }
    var ddl = context.Database.GenerateCreateScript();
    Assert.Contains("CREATE TABLE", ddl);
    Assert.Contains(modulo.ToLowerInvariant(), ddl);
    // Compila também as consultas/materializadores das entidades, sem conexão.
    foreach (var set in context.GetType().GetProperties().Where(p =>
      p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)))
      Assert.Contains("SELECT", ((IQueryable)set.GetValue(context)!).ToQueryString());
  }

  [Theory]
  [MemberData(nameof(Contextos))]
  public void ConstrutoresValidamPropriedadesEInicializamEntidades(string modulo, int _)
  {
    var assembly = Assembly.Load($"Verum.Modules.{modulo}");
    var entities = assembly.GetTypes().Where(t => t.Namespace == $"Verum.Modules.{modulo}.Dominio"
      && t.GetProperty("Id") is not null);
    foreach (var entity in entities)
    {
      Assert.True(entity.IsSealed);
      Assert.NotNull(entity.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null));
      var constructor = Assert.Single(entity.GetConstructors());
      var parameters = constructor.GetParameters();
      var arguments = parameters.Select(ValorValido).ToArray();
      // Busca/RegistroUso exigem exatamente um ator.
      for (var i = 0; i < parameters.Length; i++)
        if (parameters[i].Name == "visitanteId") arguments[i] = null;
      var instance = constructor.Invoke(arguments);
      var id = entity.GetProperty("Id")!.GetValue(instance);
      if (id is Guid guid) Assert.NotEqual(Guid.Empty, guid);
      foreach (var property in entity.GetProperties().Where(p => p.PropertyType == typeof(DateTimeOffset)))
        Assert.Equal(TimeSpan.Zero, ((DateTimeOffset)property.GetValue(instance)!).Offset);

      for (var i = 0; i < parameters.Length; i++)
      {
        var parameter = parameters[i];
        var invalid = ValorInvalido(parameter.ParameterType);
        if (!invalid.Testar) continue;
        var attempt = arguments.ToArray();
        attempt[i] = invalid.Valor;
        var exception = Assert.Throws<TargetInvocationException>(() => constructor.Invoke(attempt));
        Assert.IsAssignableFrom<ArgumentException>(exception.InnerException);
      }
    }
  }

  [Fact]
  public void VinculosCompostosProtegemProdutoEContaDaAssinatura()
  {
    using var catalogoProvider = Provider("Catalogo");
    using var catalogoScope = catalogoProvider.CreateScope();
    using var catalogo = Context(catalogoScope.ServiceProvider, "Catalogo");
    var imagem = catalogo.Model.FindEntityType(typeof(Modules.Catalogo.Dominio.ProdutoImagem))!;
    Assert.Contains(imagem.GetForeignKeys(), fk =>
      fk.Properties.Select(p => p.Name).SequenceEqual(new[] { "ProdutoVarianteId", "ProdutoId" }));
    using var assinaturasProvider = Provider("Assinaturas");
    using var assinaturasScope = assinaturasProvider.CreateScope();
    using var assinaturas = Context(assinaturasScope.ServiceProvider, "Assinaturas");
    var assinatura = assinaturas.Model.FindEntityType(typeof(Modules.Assinaturas.Dominio.Assinatura))!;
    Assert.Contains(assinatura.GetForeignKeys(), fk =>
      fk.Properties.Select(p => p.Name).SequenceEqual(new[] { "ClienteGatewayId", "ContaId", "Gateway" }));
  }

  [Fact]
  public void BuscaExigeUmUnicoAtorEExpiracaoFutura()
  {
    var now = DateTimeOffset.UtcNow;
    Assert.Throws<ArgumentException>(() => new Modules.Busca.Dominio.Busca("key", "produto", now.AddHours(1)));
    Assert.Throws<ArgumentException>(() => new Modules.Busca.Dominio.Busca("key", "produto", now.AddHours(1), Guid.NewGuid(), Guid.NewGuid()));
    Assert.Throws<ArgumentException>(() => new Modules.Busca.Dominio.Busca("key", "produto", now.AddMinutes(-1), Guid.NewGuid()));
    var busca = new Modules.Busca.Dominio.Busca("key", "  IPHONE 16  ", now.AddHours(1), Guid.NewGuid());
    Assert.Equal("iphone 16", busca.ConsultaNormalizada);
    Assert.Equal(Modules.Busca.Dominio.StatusBusca.Recebida, busca.Status);
  }

  [Fact]
  public void RankingNaoExibeMaisOfertasDoQueAnalisou()
  {
    Assert.Throws<ArgumentException>(() => new Modules.Busca.Dominio.ResultadoBusca(Guid.NewGuid(), "v1", 1, 2, false));
  }

  [Fact]
  public void JsonInvalidoEConfiancaForaDoIntervaloSaoRejeitados()
  {
    Assert.Throws<ArgumentException>(() => new Modules.Catalogo.Dominio.ProdutoVariante(Guid.NewGuid(), "nome", "slug", "{"));
    Assert.Throws<ArgumentException>(() => new Modules.Catalogo.Dominio.ProdutoVariante(Guid.NewGuid(), "nome", "slug", "[]"));
    Assert.Throws<ArgumentOutOfRangeException>(() => new Modules.Catalogo.Dominio.ProdutoTermoBusca(
      Guid.NewGuid(), "termo", Modules.Catalogo.Dominio.OrigemTermo.Catalogo, 1.01m));
  }

  [Fact]
  public void ColecoesNaoPermitemMutacaoExterna()
  {
    var categoria = new Modules.Catalogo.Dominio.Categoria("categoria", "categoria");
    var collection = Assert.IsAssignableFrom<ICollection<Modules.Catalogo.Dominio.Produto>>(categoria.Produtos);
    Assert.True(collection.IsReadOnly);
    Assert.Throws<NotSupportedException>(() => collection.Clear());
  }

  private static object? ValorValido(ParameterInfo parameter)
  {
    var type = Nullable.GetUnderlyingType(parameter.ParameterType) ?? parameter.ParameterType;
    var name = parameter.Name!;
    if (type == typeof(Guid)) return Guid.NewGuid();
    if (type.IsEnum) return Enum.GetValues(type).GetValue(0);
    if (type == typeof(bool)) return true;
    if (type == typeof(short)) return (short)1;
    if (type == typeof(int)) return 1;
    if (type == typeof(long)) return 1L;
    if (type == typeof(decimal)) return 1m;
    if (type == typeof(DateTimeOffset)) return name.Contains("expira", StringComparison.OrdinalIgnoreCase)
      || name.Contains("validaAte", StringComparison.OrdinalIgnoreCase) ? DateTimeOffset.UtcNow.AddDays(1) : DateTimeOffset.UtcNow.AddMinutes(-1);
    if (name == "email") return "teste@verum.example";
    if (name == "dominio") return "verum.example";
    if (name == "moeda") return "BRL";
    if (name.Contains("hash", StringComparison.OrdinalIgnoreCase)) return new string('a', 64);
    if (name.Contains("url", StringComparison.OrdinalIgnoreCase)) return "https://verum.example/produto";
    if (new[] { "atributos", "evidencia", "ofertaSnapshot", "dados", "conteudo", "detalhes", "configuracao" }.Contains(name))
      return "{}";
    return "valor-valido";
  }

  private static (bool Testar, object? Valor) ValorInvalido(Type propertyType)
  {
    var type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
    if (type == typeof(Guid)) return (true, Guid.Empty);
    if (type == typeof(string)) return (true, " ");
    if (type == typeof(DateTimeOffset)) return (true, default(DateTimeOffset));
    if (type.IsEnum) return (true, Enum.ToObject(type, short.MaxValue));
    if (type == typeof(decimal)) return (true, decimal.MaxValue);
    if (type == typeof(int)) return (true, -1);
    if (type == typeof(short)) return (true, (short)-1);
    if (type == typeof(long)) return (true, -1L);
    return (false, null);
  }

  private static ServiceProvider Provider(string modulo)
  {
    var services = new ServiceCollection();
    var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
      { ["ConnectionStrings:PostgreSQL"] = "Host=localhost;Database=verum_model_test;Username=test;Password=test" }).Build();
    Assembly.Load($"Verum.Modules.{modulo}").GetType($"Verum.Modules.{modulo}.DependencyInjection")!
      .GetMethod($"Add{modulo}Persistencia")!.Invoke(null, [services, configuration]);
    return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
  }

  private static DbContext Context(IServiceProvider provider, string modulo) =>
    (DbContext)provider.GetRequiredService(Assembly.Load($"Verum.Modules.{modulo}")
      .GetType($"Verum.Modules.{modulo}.Infraestrutura.Persistencia.{modulo}DbContext")!);
}
