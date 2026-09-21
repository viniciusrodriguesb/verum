namespace Verum.Modules.Busca.Dominio;

internal sealed partial class BuscaEtapa
{
  private static long ValidarId(long valor) =>
    (long)Validacao.Numero(valor, nameof(Id), 0m, long.MaxValue, 0);

  private static Guid ValidarBuscaId(Guid valor) =>
    Validacao.Identificador(valor, nameof(BuscaId));

  private static EtapaBusca ValidarEtapa(EtapaBusca valor) =>
    Validacao.Enumeracao(valor, nameof(Etapa));

  private static StatusEtapa ValidarStatus(StatusEtapa valor) =>
    Validacao.Enumeracao(valor, nameof(Status));

  private static DateTimeOffset ValidarIniciadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(IniciadaEm));

  private static DateTimeOffset? ValidarConcluidaEm(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(ConcluidaEm));

  private static string? ValidarDetalhes(string? valor) =>
    valor is null ? null : Validacao.Json(valor, nameof(Detalhes));
}

