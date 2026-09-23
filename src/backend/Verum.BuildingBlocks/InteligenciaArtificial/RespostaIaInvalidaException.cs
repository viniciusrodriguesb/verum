namespace Verum.BuildingBlocks.InteligenciaArtificial;

// Falha técnica: nunca expõe o conteúdo bruto do provedor.
public sealed class RespostaIaInvalidaException(string codigo)
  : Exception("O provedor de IA não devolveu uma resposta utilizável.")
{
  public string Codigo { get; } = codigo;
}

