namespace Verum.Modules.Ofertas.Dominio;

internal sealed partial class FonteOferta
{
  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static string ValidarNome(string valor) =>
    Validacao.Texto(valor, nameof(Nome), 120);

  private static TipoFonte ValidarTipo(TipoFonte valor) =>
    Validacao.Enumeracao(valor, nameof(Tipo));

  private static string ValidarCodigo(string valor) =>
    Validacao.Texto(valor, nameof(Codigo), 80);

  private static bool ValidarAtiva(bool valor) =>
    valor;

  private static decimal ValidarNivelConfianca(decimal valor) =>
    Validacao.Numero(valor, nameof(NivelConfianca), 0m, 100m, 2);

  private static DateTimeOffset ValidarCriadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadaEm));
}

