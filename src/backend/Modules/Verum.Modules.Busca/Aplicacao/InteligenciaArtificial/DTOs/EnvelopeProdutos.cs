using System.Text.Json;

namespace Verum.Modules.Busca.Aplicacao.InteligenciaArtificial.DTOs;

internal sealed record EnvelopeProdutos
{
  public required string ConsultaInterpretada { get; init; }

  public required string? MotivoSemResultado { get; init; }

  public required JsonElement[] Produtos { get; init; }
}
