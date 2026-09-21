namespace Verum.Modules.Acesso.Dominio;

internal sealed partial class RegistroUso
{
  public long Id { get; private set; }
  public Guid? ContaId { get; private set; }
  public Guid? VisitanteId { get; private set; }
  public string RecursoCodigo { get; private set; } = null!;
  public int Quantidade { get; private set; }
  public Guid? ReferenciaId { get; private set; }
  public string? Dados { get; private set; }
  public DateTimeOffset OcorridoEm { get; private set; }

  // Materialização pelo EF Core.
  private RegistroUso() { }

  public RegistroUso(
    string recursoCodigo,
    int quantidade,
    DateTimeOffset ocorridoEm,
    Guid? contaId = null,
    Guid? visitanteId = null,
    Guid? referenciaId = null,
    string? dados = null)
  {
    Id = ValidarId(0);
    ContaId = ValidarContaId(contaId);
    VisitanteId = ValidarVisitanteId(visitanteId);
    RecursoCodigo = ValidarRecursoCodigo(recursoCodigo);
    Quantidade = ValidarQuantidade(quantidade);
    ReferenciaId = ValidarReferenciaId(referenciaId);
    Dados = ValidarDados(dados);
    OcorridoEm = ValidarOcorridoEm(ocorridoEm);
    if (!(ContaId.HasValue != VisitanteId.HasValue)) throw new ArgumentException("Informe somente ContaId ou VisitanteId.");
  }
}

