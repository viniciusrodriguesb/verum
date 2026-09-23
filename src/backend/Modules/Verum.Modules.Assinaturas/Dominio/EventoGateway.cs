namespace Verum.Modules.Assinaturas.Dominio;

internal sealed class EventoGateway
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

  private static long ValidarId(long valor) =>
    (long)Validacao.Numero(valor, nameof(Id), 0m, long.MaxValue, 0);

  private static GatewayPagamento ValidarGateway(GatewayPagamento valor) =>
    Validacao.Enumeracao(valor, nameof(Gateway));

  private static string ValidarIdentificadorExterno(string valor) =>
    Validacao.Texto(valor, nameof(IdentificadorExterno), 200);

  private static string ValidarTipo(string valor) =>
    Validacao.Texto(valor, nameof(Tipo), 150);

  private static string ValidarConteudo(string valor) =>
    Validacao.Json(valor, nameof(Conteudo));

  private static StatusEventoGateway ValidarStatus(StatusEventoGateway valor) =>
    Validacao.Enumeracao(valor, nameof(Status));

  private static DateTimeOffset ValidarRecebidoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(RecebidoEm));

  private static DateTimeOffset? ValidarProcessadoEm(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(ProcessadoEm));

  private static short ValidarTentativas(short valor) =>
    (short)Validacao.Numero(valor, nameof(Tentativas), 0m, 32767m, 0);

  private static string? ValidarUltimoErro(string? valor) =>
    valor is null ? null : Validacao.Texto(valor, nameof(UltimoErro), 1000);
}
