namespace Verum.BuildingBlocks.Identidade;

public sealed record AtorBusca
{
  public Guid? ContaId { get; }

  public Guid? VisitanteId { get; }

  public string Chave => ContaId.HasValue ? $"conta:{ContaId}" : $"visitante:{VisitanteId}";

  private AtorBusca(Guid? contaId, Guid? visitanteId)
  {
    if ((contaId.HasValue == visitanteId.HasValue) || contaId == Guid.Empty || visitanteId == Guid.Empty)
      throw new ArgumentException("Informe uma conta ou um visitante válido.");

    ContaId = contaId;

    VisitanteId = visitanteId;
  }

  public static AtorBusca Conta(Guid id) => new(id, null);

  public static AtorBusca Visitante(Guid id) => new(null, id);
}
