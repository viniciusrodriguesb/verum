namespace Verum.Modules.Contas.Dominio;

internal sealed partial class Conta
{
  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static string ValidarSujeitoIdentidade(string valor) =>
    Validacao.Texto(valor, nameof(SujeitoIdentidade), 100);

  private static string ValidarEmail(string valor) =>
    Validacao.Email(valor, nameof(Email));

  private static string ValidarNomeExibicao(string valor) =>
    Validacao.Texto(valor, nameof(NomeExibicao), 150);

  private static StatusConta ValidarStatus(StatusConta valor) =>
    Validacao.Enumeracao(valor, nameof(Status));

  private static DateTimeOffset ValidarCriadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadaEm));

  private static DateTimeOffset ValidarAtualizadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(AtualizadaEm));

  private static DateTimeOffset? ValidarExcluidaEm(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(ExcluidaEm));
}

