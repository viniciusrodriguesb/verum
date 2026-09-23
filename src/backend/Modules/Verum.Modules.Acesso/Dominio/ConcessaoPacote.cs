using Verum.BuildingBlocks.Erros;

namespace Verum.Modules.Acesso.Dominio;

internal sealed class ConcessaoPacote
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


  public PacoteAcesso Pacote { get; private set; } = null!;

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

    if (!(ValidaAte is null || ValidaAte > ValidaDe))
      throw ErroAplicacaoException.Validacao("ValidaAte deve ser posterior a ValidaDe.");
  }

  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static Guid ValidarContaId(Guid valor) =>
    Validacao.Identificador(valor, nameof(ContaId));

  private static Guid ValidarPacoteAcessoId(Guid valor) =>
    Validacao.Identificador(valor, nameof(PacoteAcessoId));

  private static OrigemConcessao ValidarOrigem(OrigemConcessao valor) =>
    Validacao.Enumeracao(valor, nameof(Origem));

  private static Guid? ValidarReferenciaOrigemId(Guid? valor) =>
    valor is null ? null : Validacao.Identificador(valor.Value, nameof(ReferenciaOrigemId));

  private static DateTimeOffset ValidarValidaDe(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(ValidaDe));

  private static DateTimeOffset? ValidarValidaAte(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(ValidaAte));

  private static DateTimeOffset? ValidarRevogadaEm(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(RevogadaEm));

  private static DateTimeOffset ValidarCriadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadaEm));
}
