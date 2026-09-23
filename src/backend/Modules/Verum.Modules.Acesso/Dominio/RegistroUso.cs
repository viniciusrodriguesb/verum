using Verum.BuildingBlocks.Erros;

namespace Verum.Modules.Acesso.Dominio;

internal sealed class RegistroUso
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

    if (!(ContaId.HasValue != VisitanteId.HasValue)) throw ErroAplicacaoException.Validacao("Informe somente ContaId ou VisitanteId.");
  }

  private static long ValidarId(long valor) =>
    (long)Validacao.Numero(valor, nameof(Id), 0m, long.MaxValue, 0);

  private static Guid? ValidarContaId(Guid? valor) =>
    valor is null ? null : Validacao.Identificador(valor.Value, nameof(ContaId));

  private static Guid? ValidarVisitanteId(Guid? valor) =>
    valor is null ? null : Validacao.Identificador(valor.Value, nameof(VisitanteId));

  private static string ValidarRecursoCodigo(string valor) =>
    Validacao.Texto(valor, nameof(RecursoCodigo), 100);

  private static int ValidarQuantidade(int valor) =>
    (int)Validacao.Numero(valor, nameof(Quantidade), 1m, 2147483647m, 0);

  private static Guid? ValidarReferenciaId(Guid? valor) =>
    valor is null ? null : Validacao.Identificador(valor.Value, nameof(ReferenciaId));

  private static string? ValidarDados(string? valor) =>
    valor is null ? null : Validacao.Json(valor, nameof(Dados));

  private static DateTimeOffset ValidarOcorridoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(OcorridoEm));
}
