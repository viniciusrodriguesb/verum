namespace Verum.Modules.Acesso.Dominio;

internal sealed partial class RegistroUso
{
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

