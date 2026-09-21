using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Verum.CrossCutting.Pipelines;

public static class ServicesPipeline
{
  // Somente assemblies explicitamente selecionados pelo host são inspecionados.
  // Classes concretas fechadas em Aplicacao, terminadas em Service, são scoped.
  public static IServiceCollection AddVerumServices(this IServiceCollection services, params Assembly[] assemblies)
  {
    var types = assemblies.Distinct().SelectMany(a => a.GetTypes())
      .Where(t => t.IsClass && !t.IsAbstract && !t.ContainsGenericParameters
        && t.Name.EndsWith("Service", StringComparison.Ordinal)
        && t.Namespace is not null && t.Namespace.Split('.').Contains("Aplicacao"))
      .OrderBy(t => t.FullName, StringComparer.Ordinal);

    foreach (var type in types)
    {
      var contract = type.GetInterfaces().SingleOrDefault(i => i.Name == $"I{type.Name}");
      if (contract is not null && services.Any(d => d.ServiceType == contract))
      {
        var existing = services.SingleOrDefault(d => d.ServiceType == type);
        if (existing?.ImplementationType == type) continue;
        throw new InvalidOperationException($"Registro ambíguo para {contract.FullName}.");
      }

      if (!services.Any(d => d.ServiceType == type))
        services.AddScoped(type);
      if (contract is not null)
        services.AddScoped(contract, provider => provider.GetRequiredService(type));
    }

    return services;
  }
}

