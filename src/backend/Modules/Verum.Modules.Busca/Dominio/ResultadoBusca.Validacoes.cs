namespace Verum.Modules.Busca.Dominio;

internal sealed partial class ResultadoBusca
{
  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static Guid ValidarBuscaId(Guid valor) =>
    Validacao.Identificador(valor, nameof(BuscaId));

  private static string ValidarVersaoRanking(string valor) =>
    Validacao.Texto(valor, nameof(VersaoRanking), 50);

  private static int ValidarQuantidadeAnalisada(int valor) =>
    (int)Validacao.Numero(valor, nameof(QuantidadeAnalisada), 0m, 2147483647m, 0);

  private static int ValidarQuantidadeExibida(int valor) =>
    (int)Validacao.Numero(valor, nameof(QuantidadeExibida), 0m, 2147483647m, 0);

  private static bool ValidarParcial(bool valor) =>
    valor;

  private static DateTimeOffset ValidarGeradoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(GeradoEm));
}

