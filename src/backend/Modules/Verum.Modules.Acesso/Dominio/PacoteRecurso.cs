namespace Verum.Modules.Acesso.Dominio;

internal sealed partial class PacoteRecurso
{
  public Guid Id { get; private set; }
  public Guid PacoteAcessoId { get; private set; }
  public Guid RecursoId { get; private set; }
  public bool Habilitado { get; private set; }
  public int? LimiteQuantidade { get; private set; }
  public PeriodicidadeUso? Periodicidade { get; private set; }
  public string? Configuracao { get; private set; }

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
}

