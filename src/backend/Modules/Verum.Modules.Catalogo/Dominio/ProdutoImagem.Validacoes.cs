namespace Verum.Modules.Catalogo.Dominio;

internal sealed partial class ProdutoImagem
{
  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static Guid ValidarProdutoId(Guid valor) =>
    Validacao.Identificador(valor, nameof(ProdutoId));

  private static Guid? ValidarProdutoVarianteId(Guid? valor) =>
    valor is null ? null : Validacao.Identificador(valor.Value, nameof(ProdutoVarianteId));

  private static string ValidarUrl(string valor) =>
    Validacao.Url(valor, nameof(Url));

  private static string ValidarOrigem(string valor) =>
    Validacao.Texto(valor, nameof(Origem), 100);

  private static bool ValidarPrincipal(bool valor) =>
    valor;

  private static short ValidarOrdem(short valor) =>
    (short)Validacao.Numero(valor, nameof(Ordem), 0m, 32767m, 0);

  private static DateTimeOffset ValidarCriadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadaEm));
}

