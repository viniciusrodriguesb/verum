namespace Verum.Modules.Busca.Dominio;

internal sealed partial class Busca
{
  public Guid Id { get; private set; }
  public Guid? ContaId { get; private set; }
  public Guid? VisitanteId { get; private set; }
  public string ChaveIdempotencia { get; private set; } = null!;
  public string ConsultaOriginal { get; private set; } = null!;
  public string ConsultaNormalizada { get; private set; } = null!;
  public string? ConsultaEstruturada { get; private set; }
  public Guid? ProdutoPrincipalId { get; private set; }
  public Guid? ProdutoVariantePrincipalId { get; private set; }
  public StatusBusca Status { get; private set; }
  public EtapaBusca EtapaAtual { get; private set; }
  public MotivoBuscaSemResultado? MotivoSemResultado { get; private set; }
  public bool ResultadoParcial { get; private set; }
  public string? CodigoErro { get; private set; }
  public string? MensagemErro { get; private set; }
  public DateTimeOffset IniciadaEm { get; private set; }
  public DateTimeOffset? ConcluidaEm { get; private set; }
  public DateTimeOffset ExpiraEm { get; private set; }
  public int QuantidadeFontesConsultadas { get; private set; }
  public int QuantidadeFontesComSucesso { get; private set; }
  public int QuantidadeFontesComFalha { get; private set; }

  // Materialização pelo EF Core.
  private Busca() { }

  public Busca(
    string chaveIdempotencia,
    string consultaOriginal,
    DateTimeOffset expiraEm,
    Guid? contaId = null,
    Guid? visitanteId = null)
  {
    var agora = DateTimeOffset.UtcNow;
    Id = ValidarId(Guid.CreateVersion7());
    ContaId = ValidarContaId(contaId);
    VisitanteId = ValidarVisitanteId(visitanteId);
    ChaveIdempotencia = ValidarChaveIdempotencia(chaveIdempotencia);
    ConsultaOriginal = ValidarConsultaOriginal(consultaOriginal);
    ConsultaNormalizada = ValidarConsultaNormalizada(ConsultaOriginal.ToLowerInvariant());
    ConsultaEstruturada = ValidarConsultaEstruturada(null);
    ProdutoPrincipalId = ValidarProdutoPrincipalId(null);
    ProdutoVariantePrincipalId = ValidarProdutoVariantePrincipalId(null);
    Status = ValidarStatus(StatusBusca.Recebida);
    EtapaAtual = ValidarEtapaAtual(EtapaBusca.Recepcao);
    MotivoSemResultado = ValidarMotivoSemResultado(null);
    ResultadoParcial = ValidarResultadoParcial(false);
    CodigoErro = ValidarCodigoErro(null);
    MensagemErro = ValidarMensagemErro(null);
    IniciadaEm = ValidarIniciadaEm(agora);
    ConcluidaEm = ValidarConcluidaEm(null);
    ExpiraEm = ValidarExpiraEm(expiraEm);
    QuantidadeFontesConsultadas = ValidarQuantidadeFontesConsultadas(0);
    QuantidadeFontesComSucesso = ValidarQuantidadeFontesComSucesso(0);
    QuantidadeFontesComFalha = ValidarQuantidadeFontesComFalha(0);
    if (!(ContaId.HasValue != VisitanteId.HasValue)) throw new ArgumentException("Informe somente ContaId ou VisitanteId.");
    if (!(ExpiraEm > IniciadaEm)) throw new ArgumentException("ExpiraEm deve ser posterior ao início.");
  }
}

