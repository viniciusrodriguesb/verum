namespace Verum.Modules.Acesso.Dominio;

internal sealed partial class Recurso
{
  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static string ValidarCodigo(string valor) =>
    Validacao.Texto(valor, nameof(Codigo), 100);

  private static string ValidarNome(string valor) =>
    Validacao.Texto(valor, nameof(Nome), 150);

  private static string ValidarDescricao(string valor) =>
    Validacao.Texto(valor, nameof(Descricao), 500);

  private static TipoLimite ValidarTipoLimite(TipoLimite valor) =>
    Validacao.Enumeracao(valor, nameof(TipoLimite));

  private static bool ValidarAtivo(bool valor) =>
    valor;

  private static DateTimeOffset ValidarCriadoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadoEm));
}

