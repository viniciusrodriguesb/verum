namespace Verum.Modules.Acesso.Dominio;

internal sealed partial class ConcessaoPacote
{
  public Guid Id { get; private set; }
  public Guid ContaId { get; private set; }
  public Guid PacoteAcessoId { get; private set; }
  public OrigemConcessao Origem { get; private set; }
  public Guid? ReferenciaOrigemId { get; private set; }
  public DateTimeOffset ValidaDe { get; private set; }
  public DateTimeOffset? ValidaAte { get; private set; }
  public DateTimeOffset? RevogadaEm { get; private set; }
  public DateTimeOffset CriadaEm { get; private set; }

  // Materialização pelo EF Core.
  private ConcessaoPacote() { }

  public ConcessaoPacote(
    Guid contaId,
    Guid pacoteAcessoId,
    OrigemConcessao origem,
    DateTimeOffset validaDe,
    Guid? referenciaOrigemId = null,
    DateTimeOffset? validaAte = null)
  {
    var agora = DateTimeOffset.UtcNow;
    Id = ValidarId(Guid.CreateVersion7());
    ContaId = ValidarContaId(contaId);
    PacoteAcessoId = ValidarPacoteAcessoId(pacoteAcessoId);
    Origem = ValidarOrigem(origem);
    ReferenciaOrigemId = ValidarReferenciaOrigemId(referenciaOrigemId);
    ValidaDe = ValidarValidaDe(validaDe);
    ValidaAte = ValidarValidaAte(validaAte);
    RevogadaEm = ValidarRevogadaEm(null);
    CriadaEm = ValidarCriadaEm(agora);
    if (!(ValidaAte is null || ValidaAte > ValidaDe)) throw new ArgumentException("ValidaAte deve ser posterior a ValidaDe.");
  }
}

