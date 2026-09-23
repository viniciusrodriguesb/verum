namespace Verum.Modules.Ofertas.Dominio;

internal sealed class ExecucaoConsultaFonte
{
  public Guid Id { get; private set; }

  public Guid CorrelacaoId { get; private set; }

  public Guid BuscaId { get; private set; }

  public Guid FonteOfertaId { get; private set; }

  public TipoFonte TipoProvedor { get; private set; }

  public StatusConsultaFonte Status { get; private set; }

  public int QuantidadeItensEncontrados { get; private set; }

  public int QuantidadeItensAceitos { get; private set; }

  public DateTimeOffset IniciadaEm { get; private set; }

  public DateTimeOffset? FinalizadaEm { get; private set; }

  public short NumeroTentativas { get; private set; }

  public string? CodigoErro { get; private set; }

  public string? DetalhesErro { get; private set; }


  public FonteOferta Fonte { get; private set; } = null!;

  // Materialização pelo EF Core.
  private ExecucaoConsultaFonte() { }

  public ExecucaoConsultaFonte(
    Guid correlacaoId,
    Guid buscaId,
    Guid fonteOfertaId,
    TipoFonte tipoProvedor)
  {
    var agora = DateTimeOffset.UtcNow;

    Id = ValidarId(Guid.CreateVersion7());

    CorrelacaoId = ValidarCorrelacaoId(correlacaoId);

    BuscaId = ValidarBuscaId(buscaId);

    FonteOfertaId = ValidarFonteOfertaId(fonteOfertaId);

    TipoProvedor = ValidarTipoProvedor(tipoProvedor);

    Status = ValidarStatus(StatusConsultaFonte.Pendente);

    QuantidadeItensEncontrados = ValidarQuantidadeItensEncontrados(0);

    QuantidadeItensAceitos = ValidarQuantidadeItensAceitos(0);

    IniciadaEm = ValidarIniciadaEm(agora);

    FinalizadaEm = ValidarFinalizadaEm(null);

    NumeroTentativas = ValidarNumeroTentativas(0);

    CodigoErro = ValidarCodigoErro(null);

    DetalhesErro = ValidarDetalhesErro(null);
  }

  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static Guid ValidarCorrelacaoId(Guid valor) =>
    Validacao.Identificador(valor, nameof(CorrelacaoId));

  private static Guid ValidarBuscaId(Guid valor) =>
    Validacao.Identificador(valor, nameof(BuscaId));

  private static Guid ValidarFonteOfertaId(Guid valor) =>
    Validacao.Identificador(valor, nameof(FonteOfertaId));

  private static TipoFonte ValidarTipoProvedor(TipoFonte valor) =>
    Validacao.Enumeracao(valor, nameof(TipoProvedor));

  private static StatusConsultaFonte ValidarStatus(StatusConsultaFonte valor) =>
    Validacao.Enumeracao(valor, nameof(Status));

  private static int ValidarQuantidadeItensEncontrados(int valor) =>
    (int)Validacao.Numero(valor, nameof(QuantidadeItensEncontrados), 0m, 2147483647m, 0);

  private static int ValidarQuantidadeItensAceitos(int valor) =>
    (int)Validacao.Numero(valor, nameof(QuantidadeItensAceitos), 0m, 2147483647m, 0);

  private static DateTimeOffset ValidarIniciadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(IniciadaEm));

  private static DateTimeOffset? ValidarFinalizadaEm(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(FinalizadaEm));

  private static short ValidarNumeroTentativas(short valor) =>
    (short)Validacao.Numero(valor, nameof(NumeroTentativas), 0m, 32767m, 0);

  private static string? ValidarCodigoErro(string? valor) =>
    valor is null ? null : Validacao.Texto(valor, nameof(CodigoErro), 100);

  private static string? ValidarDetalhesErro(string? valor) =>
    valor is null ? null : Validacao.Texto(valor, nameof(DetalhesErro), 1000);
}
