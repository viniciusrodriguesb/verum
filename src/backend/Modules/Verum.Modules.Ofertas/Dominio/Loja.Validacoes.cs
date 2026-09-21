namespace Verum.Modules.Ofertas.Dominio;

internal sealed partial class Loja
{
  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static string ValidarNome(string valor) =>
    Validacao.Texto(valor, nameof(Nome), 180);

  private static string ValidarNomeNormalizado(string valor) =>
    Validacao.Texto(valor, nameof(NomeNormalizado), 180);

  private static string? ValidarDominio(string? valor) =>
    valor is null ? null : Validacao.Dominio(valor, nameof(Dominio));

  private static string? ValidarUrl(string? valor) =>
    valor is null ? null : Validacao.Url(valor, nameof(Url));

  private static bool ValidarVerificada(bool valor) =>
    valor;

  private static decimal ValidarPontuacaoConfianca(decimal valor) =>
    Validacao.Numero(valor, nameof(PontuacaoConfianca), 0m, 100m, 2);

  private static StatusLoja ValidarStatus(StatusLoja valor) =>
    Validacao.Enumeracao(valor, nameof(Status));

  private static DateTimeOffset ValidarCriadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadaEm));

  private static DateTimeOffset ValidarAtualizadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(AtualizadaEm));
}

