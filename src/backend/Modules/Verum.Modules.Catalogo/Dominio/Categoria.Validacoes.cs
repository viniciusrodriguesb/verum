namespace Verum.Modules.Catalogo.Dominio;

internal sealed partial class Categoria
{
  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static Guid? ValidarCategoriaPaiId(Guid? valor) =>
    valor is null ? null : Validacao.Identificador(valor.Value, nameof(CategoriaPaiId));

  private static string ValidarNome(string valor) =>
    Validacao.Texto(valor, nameof(Nome), 120);

  private static string ValidarSlug(string valor) =>
    Validacao.Texto(valor, nameof(Slug), 140);

  private static bool ValidarAtiva(bool valor) =>
    valor;

  private static DateTimeOffset ValidarCriadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadaEm));

  private static DateTimeOffset ValidarAtualizadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(AtualizadaEm));
}

