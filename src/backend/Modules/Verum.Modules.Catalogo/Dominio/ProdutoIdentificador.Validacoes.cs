namespace Verum.Modules.Catalogo.Dominio;

internal sealed partial class ProdutoIdentificador
{
  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static Guid ValidarProdutoVarianteId(Guid valor) =>
    Validacao.Identificador(valor, nameof(ProdutoVarianteId));

  private static TipoIdentificador ValidarTipo(TipoIdentificador valor) =>
    Validacao.Enumeracao(valor, nameof(Tipo));

  private static string ValidarValor(string valor) =>
    Validacao.Texto(valor, nameof(Valor), 100);

  private static DateTimeOffset ValidarCriadoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadoEm));
}

