namespace Verum.Modules.Ofertas.Dominio;

internal sealed partial class ExecucaoConsultaFonte
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
}

