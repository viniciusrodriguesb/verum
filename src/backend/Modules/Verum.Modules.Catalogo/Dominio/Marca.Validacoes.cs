namespace Verum.Modules.Catalogo.Dominio;

internal sealed partial class Marca
{
  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static string ValidarNome(string valor) =>
    Validacao.Texto(valor, nameof(Nome), 120);

  private static string ValidarNomeNormalizado(string valor) =>
    Validacao.Texto(valor, nameof(NomeNormalizado), 120);

  private static string ValidarSlug(string valor) =>
    Validacao.Texto(valor, nameof(Slug), 140);

  private static bool ValidarAtiva(bool valor) =>
    valor;

  private static DateTimeOffset ValidarCriadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadaEm));
}

