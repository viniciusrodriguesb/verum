namespace Verum.Modules.Acesso.Dominio;

internal sealed partial class PacoteRecurso
{
  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static Guid ValidarPacoteAcessoId(Guid valor) =>
    Validacao.Identificador(valor, nameof(PacoteAcessoId));

  private static Guid ValidarRecursoId(Guid valor) =>
    Validacao.Identificador(valor, nameof(RecursoId));

  private static bool ValidarHabilitado(bool valor) =>
    valor;

  private static int? ValidarLimiteQuantidade(int? valor) =>
    valor is null ? null : (int)Validacao.Numero(valor.Value, nameof(LimiteQuantidade), 0m, 2147483647m, 0);

  private static PeriodicidadeUso? ValidarPeriodicidade(PeriodicidadeUso? valor) =>
    valor is null ? null : Validacao.Enumeracao(valor.Value, nameof(Periodicidade));

  private static string? ValidarConfiguracao(string? valor) =>
    valor is null ? null : Validacao.Json(valor, nameof(Configuracao));
}

