namespace Verum.Modules.Radar.Dominio;

internal sealed partial class Monitoramento
{
  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static Guid ValidarContaId(Guid valor) =>
    Validacao.Identificador(valor, nameof(ContaId));

  private static Guid ValidarProdutoId(Guid valor) =>
    Validacao.Identificador(valor, nameof(ProdutoId));

  private static Guid? ValidarProdutoVarianteId(Guid? valor) =>
    valor is null ? null : Validacao.Identificador(valor.Value, nameof(ProdutoVarianteId));

  private static string ValidarNome(string valor) =>
    Validacao.Texto(valor, nameof(Nome), 250);

  private static decimal ValidarPrecoInicial(decimal valor) =>
    Validacao.Numero(valor, nameof(PrecoInicial), 0.01m, 999999999999.99m, 2);

  private static decimal ValidarPrecoAlvo(decimal valor) =>
    Validacao.Numero(valor, nameof(PrecoAlvo), 0.01m, 999999999999.99m, 2);

  private static decimal? ValidarMenorPrecoAtual(decimal? valor) =>
    valor is null ? null : Validacao.Numero(valor.Value, nameof(MenorPrecoAtual), 0m, 999999999999.99m, 2);

  private static Guid? ValidarUltimaOfertaId(Guid? valor) =>
    valor is null ? null : Validacao.Identificador(valor.Value, nameof(UltimaOfertaId));

  private static long? ValidarUltimaObservacaoId(long? valor) =>
    valor is null ? null : (long)Validacao.Numero(valor.Value, nameof(UltimaObservacaoId), 1m, long.MaxValue, 0);

  private static StatusMonitoramento ValidarStatus(StatusMonitoramento valor) =>
    Validacao.Enumeracao(valor, nameof(Status));

  private static DateTimeOffset? ValidarUltimaVerificacaoEm(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(UltimaVerificacaoEm));

  private static DateTimeOffset? ValidarProximaVerificacaoEm(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(ProximaVerificacaoEm));

  private static DateTimeOffset ValidarCriadoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadoEm));

  private static DateTimeOffset ValidarAtualizadoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(AtualizadoEm));

  private static DateTimeOffset? ValidarPausadoEm(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(PausadoEm));

  private static DateTimeOffset? ValidarExcluidoEm(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(ExcluidoEm));

  private static DateTimeOffset? ValidarUltimoAlertaEm(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(UltimoAlertaEm));

  private static decimal? ValidarUltimoPrecoAlertado(decimal? valor) =>
    valor is null ? null : Validacao.Numero(valor.Value, nameof(UltimoPrecoAlertado), 0m, 999999999999.99m, 2);
}

