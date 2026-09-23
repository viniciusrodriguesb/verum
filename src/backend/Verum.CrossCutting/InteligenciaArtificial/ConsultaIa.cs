using Microsoft.Extensions.Options;
using Verum.BuildingBlocks.InteligenciaArtificial;

namespace Verum.CrossCutting.InteligenciaArtificial;

internal sealed class ConsultaIaService(
  IEnumerable<IProvedorIa> provedores,
  IOptions<InteligenciaArtificialOptions> options) : IConsultaIa
{
  public Task<RespostaIa> ConsultarAsync(BuildingBlocks.InteligenciaArtificial.ConsultaIa consulta, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(consulta);

    if (!options.Value.Enabled)
      throw new InvalidOperationException("Habilite InteligenciaArtificial:Enabled para consultar IA.");

    var nome = consulta.Provedor ?? options.Value.ProvedorPadrao;

    var correspondentes = provedores.Where(x => string.Equals(x.Nome, nome, StringComparison.OrdinalIgnoreCase)).ToArray();

    if (correspondentes.Length != 1)
      throw new InvalidOperationException("O provedor de IA deve possuir exatamente um registro.");

    return correspondentes[0].ConsultarAsync(consulta, cancellationToken);
  }
}

