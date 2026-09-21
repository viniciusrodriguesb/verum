namespace Verum.Modules.Catalogo.Dominio;

internal sealed partial class ProdutoTermoBusca
{
  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static Guid ValidarProdutoId(Guid valor) =>
    Validacao.Identificador(valor, nameof(ProdutoId));

  private static Guid? ValidarProdutoVarianteId(Guid? valor) =>
    valor is null ? null : Validacao.Identificador(valor.Value, nameof(ProdutoVarianteId));

  private static string ValidarTermoOriginal(string valor) =>
    Validacao.Texto(valor, nameof(TermoOriginal), 300);

  private static string ValidarTermoNormalizado(string valor) =>
    Validacao.Texto(valor, nameof(TermoNormalizado), 300);

  private static OrigemTermo ValidarOrigem(OrigemTermo valor) =>
    Validacao.Enumeracao(valor, nameof(Origem));

  private static decimal ValidarConfianca(decimal valor) =>
    Validacao.Numero(valor, nameof(Confianca), 0m, 1m, 4);

  private static DateTimeOffset ValidarCriadoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadoEm));
}

