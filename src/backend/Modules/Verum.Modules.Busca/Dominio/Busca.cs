using Verum.BuildingBlocks.Erros;

namespace Verum.Modules.Busca.Dominio;

internal sealed class Busca
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

  private readonly List<BuscaEtapa> _etapas = [];

  public IReadOnlyCollection<BuscaEtapa> Etapas => _etapas.AsReadOnly();

  public ResultadoBusca? Resultado { get; private set; }

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

    if (!(ContaId.HasValue != VisitanteId.HasValue)) throw ErroAplicacaoException.Validacao("Informe somente ContaId ou VisitanteId.");

    if (!(ExpiraEm > IniciadaEm)) throw ErroAplicacaoException.Validacao("ExpiraEm deve ser posterior ao início.");
  }

  public void Iniciar()
  {
    if (ConcluidaEm.HasValue) throw new InvalidOperationException("Busca já finalizada.");

    Status = StatusBusca.Processando;

    EtapaAtual = EtapaBusca.Catalogo;
  }

  public void Avancar(EtapaBusca etapa)
  {
    if (Status != StatusBusca.Processando) throw new InvalidOperationException("Busca não está em processamento.");

    EtapaAtual = ValidarEtapaAtual(etapa);
  }

  public void RegistrarFalhaTecnica()
  {
    if (ConcluidaEm.HasValue) return;

    Status = ExpiraEm <= DateTimeOffset.UtcNow ? StatusBusca.Expirada : StatusBusca.Falhou;

    CodigoErro = Status == StatusBusca.Expirada ? "PRAZO_ESGOTADO" : "PROCESSAMENTO_FALHOU";

    ConcluidaEm = DateTimeOffset.UtcNow;
  }

  public void Finalizar(int quantidade, bool parcial, int sucesso, int falha, bool expirada = false,
    MotivoBuscaSemResultado? motivo = null)
  {
    QuantidadeFontesComSucesso = ValidarQuantidadeFontesComSucesso(sucesso);

    QuantidadeFontesComFalha = ValidarQuantidadeFontesComFalha(falha);

    QuantidadeFontesConsultadas = sucesso + falha;

    ResultadoParcial = parcial;

    Status = expirada ? StatusBusca.Expirada : quantidade > 0 ? StatusBusca.Concluida
      : sucesso == 0 && motivo is null ? StatusBusca.Falhou : StatusBusca.ConcluidaSemResultado;

    CodigoErro = Status == StatusBusca.Falhou ? "FONTES_INDISPONIVEIS" : expirada ? "PRAZO_ESGOTADO" : null;

    MotivoSemResultado = Status == StatusBusca.ConcluidaSemResultado ? motivo ?? MotivoBuscaSemResultado.ProdutoSemOfertas : null;

    EtapaAtual = EtapaBusca.Resultado;

    ConcluidaEm = DateTimeOffset.UtcNow;
  }

  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static Guid? ValidarContaId(Guid? valor) =>
    valor is null ? null : Validacao.Identificador(valor.Value, nameof(ContaId));

  private static Guid? ValidarVisitanteId(Guid? valor) =>
    valor is null ? null : Validacao.Identificador(valor.Value, nameof(VisitanteId));

  private static string ValidarChaveIdempotencia(string valor) =>
    Validacao.Texto(valor, nameof(ChaveIdempotencia), 100);

  private static string ValidarConsultaOriginal(string valor) =>
    Validacao.Texto(valor, nameof(ConsultaOriginal), 500);

  private static string ValidarConsultaNormalizada(string valor) =>
    Validacao.Texto(valor, nameof(ConsultaNormalizada), 500);

  private static string? ValidarConsultaEstruturada(string? valor) =>
    valor is null ? null : Validacao.Json(valor, nameof(ConsultaEstruturada));

  private static Guid? ValidarProdutoPrincipalId(Guid? valor) =>
    valor is null ? null : Validacao.Identificador(valor.Value, nameof(ProdutoPrincipalId));

  private static Guid? ValidarProdutoVariantePrincipalId(Guid? valor) =>
    valor is null ? null : Validacao.Identificador(valor.Value, nameof(ProdutoVariantePrincipalId));

  private static StatusBusca ValidarStatus(StatusBusca valor) =>
    Validacao.Enumeracao(valor, nameof(Status));

  private static EtapaBusca ValidarEtapaAtual(EtapaBusca valor) =>
    Validacao.Enumeracao(valor, nameof(EtapaAtual));

  private static MotivoBuscaSemResultado? ValidarMotivoSemResultado(MotivoBuscaSemResultado? valor) =>
    valor is null ? null : Validacao.Enumeracao(valor.Value, nameof(MotivoSemResultado));

  private static bool ValidarResultadoParcial(bool valor) =>
    valor;

  private static string? ValidarCodigoErro(string? valor) =>
    valor is null ? null : Validacao.Texto(valor, nameof(CodigoErro), 100);

  private static string? ValidarMensagemErro(string? valor) =>
    valor is null ? null : Validacao.Texto(valor, nameof(MensagemErro), 500);

  private static DateTimeOffset ValidarIniciadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(IniciadaEm));

  private static DateTimeOffset? ValidarConcluidaEm(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(ConcluidaEm));

  private static DateTimeOffset ValidarExpiraEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(ExpiraEm));

  private static int ValidarQuantidadeFontesConsultadas(int valor) =>
    (int)Validacao.Numero(valor, nameof(QuantidadeFontesConsultadas), 0m, 2147483647m, 0);

  private static int ValidarQuantidadeFontesComSucesso(int valor) =>
    (int)Validacao.Numero(valor, nameof(QuantidadeFontesComSucesso), 0m, 2147483647m, 0);

  private static int ValidarQuantidadeFontesComFalha(int valor) =>
    (int)Validacao.Numero(valor, nameof(QuantidadeFontesComFalha), 0m, 2147483647m, 0);
}
