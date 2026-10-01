namespace Verum.Modules.Busca.Contratos;

// A mensagem referencia a busca persistida; o processamento carregará seus dados.
public sealed record BuscaSolicitada(Guid BuscaId)
{
  public const string FilaPadrao = "verum-descoberta-busca-solicitada";
}
