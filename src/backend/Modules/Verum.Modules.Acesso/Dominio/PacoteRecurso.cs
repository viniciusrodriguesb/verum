namespace Verum.Modules.Acesso.Dominio;

internal sealed class PacoteRecurso
{
  public Guid Id { get; private set; }

  public Guid PacoteAcessoId { get; private set; }

  public Guid RecursoId { get; private set; }

  public bool Habilitado { get; private set; }

  public int? LimiteQuantidade { get; private set; }

  public PeriodicidadeUso? Periodicidade { get; private set; }

  public string? Configuracao { get; private set; }


  public PacoteAcesso Pacote { get; private set; } = null!;

  public Recurso Recurso { get; private set; } = null!;

  // Materialização pelo EF Core.
  private PacoteRecurso() { }

  public PacoteRecurso(
    Guid pacoteAcessoId,
    Guid recursoId,
    bool habilitado,
    int? limiteQuantidade = null,
    PeriodicidadeUso? periodicidade = null,
    string? configuracao = null)
  {
    Id = ValidarId(Guid.CreateVersion7());

    PacoteAcessoId = ValidarPacoteAcessoId(pacoteAcessoId);

    RecursoId = ValidarRecursoId(recursoId);

    Habilitado = ValidarHabilitado(habilitado);

    LimiteQuantidade = ValidarLimiteQuantidade(limiteQuantidade);

    Periodicidade = ValidarPeriodicidade(periodicidade);

    Configuracao = ValidarConfiguracao(configuracao);
  }

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
