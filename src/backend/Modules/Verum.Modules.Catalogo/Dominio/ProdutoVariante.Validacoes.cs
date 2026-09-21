namespace Verum.Modules.Catalogo.Dominio;

internal sealed partial class ProdutoVariante
{
  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static Guid ValidarProdutoId(Guid valor) =>
    Validacao.Identificador(valor, nameof(ProdutoId));

  private static string ValidarNome(string valor) =>
    Validacao.Texto(valor, nameof(Nome), 300);

  private static string ValidarNomeNormalizado(string valor) =>
    Validacao.Texto(valor, nameof(NomeNormalizado), 300);

  private static string ValidarSlug(string valor) =>
    Validacao.Texto(valor, nameof(Slug), 320);

  private static string ValidarAtributos(string valor) =>
    Validacao.Json(valor, nameof(Atributos));

  private static StatusProduto ValidarStatus(StatusProduto valor) =>
    Validacao.Enumeracao(valor, nameof(Status));

  private static DateTimeOffset ValidarCriadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadaEm));

  private static DateTimeOffset ValidarAtualizadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(AtualizadaEm));
}

