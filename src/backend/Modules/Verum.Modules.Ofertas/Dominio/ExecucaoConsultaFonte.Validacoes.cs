namespace Verum.Modules.Ofertas.Dominio;

internal sealed partial class ExecucaoConsultaFonte
{
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

