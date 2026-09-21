namespace Verum.Modules.Assinaturas.Dominio;

internal sealed partial class EventoGateway
{
  public long Id { get; private set; }
  public GatewayPagamento Gateway { get; private set; }
  public string IdentificadorExterno { get; private set; } = null!;
  public string Tipo { get; private set; } = null!;
  public string Conteudo { get; private set; } = null!;
  public StatusEventoGateway Status { get; private set; }
  public DateTimeOffset RecebidoEm { get; private set; }
  public DateTimeOffset? ProcessadoEm { get; private set; }
  public short Tentativas { get; private set; }
  public string? UltimoErro { get; private set; }

  // Materialização pelo EF Core.
  private EventoGateway() { }

  public EventoGateway(
    GatewayPagamento gateway,
    string identificadorExterno,
    string tipo,
    string conteudo)
  {
    var agora = DateTimeOffset.UtcNow;
    Id = ValidarId(0);
    Gateway = ValidarGateway(gateway);
    IdentificadorExterno = ValidarIdentificadorExterno(identificadorExterno);
    Tipo = ValidarTipo(tipo);
    Conteudo = ValidarConteudo(conteudo);
    Status = ValidarStatus(StatusEventoGateway.Pendente);
    RecebidoEm = ValidarRecebidoEm(agora);
    ProcessadoEm = ValidarProcessadoEm(null);
    Tentativas = ValidarTentativas(0);
    UltimoErro = ValidarUltimoErro(null);

  }
}

