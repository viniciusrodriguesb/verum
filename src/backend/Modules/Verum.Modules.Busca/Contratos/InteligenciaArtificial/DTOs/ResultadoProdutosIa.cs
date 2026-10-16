namespace Verum.Modules.Busca.Contratos.InteligenciaArtificial.DTOs;

public sealed record ResultadoProdutosIa
{
  public required string ConsultaInterpretada { get; init; }

  public required string? MotivoSemResultado { get; init; }

  public required ProdutoCandidatoIa[] Produtos { get; init; }
}
