using Verum.BuildingBlocks.InteligenciaArtificial;
using Verum.BuildingBlocks.InteligenciaArtificial.DTOs;

namespace Verum.Modules.Busca.Contratos.InteligenciaArtificial.DTOs;

public sealed record DescobertaProdutosIa(ResultadoProdutosIa Resultado, RespostaIa Execucao)
{
  public IReadOnlyList<CandidatoIaDescartado> Descartados { get; init; } = [];
}

// Índice baseado em zero no array original; não guarda o conteúdo externo inválido.
