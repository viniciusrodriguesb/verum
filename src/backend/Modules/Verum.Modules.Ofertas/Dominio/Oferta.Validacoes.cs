namespace Verum.Modules.Ofertas.Dominio;

internal sealed partial class Oferta
{
  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static Guid ValidarProdutoVarianteId(Guid valor) =>
    Validacao.Identificador(valor, nameof(ProdutoVarianteId));

  private static Guid ValidarLojaId(Guid valor) =>
    Validacao.Identificador(valor, nameof(LojaId));

  private static Guid ValidarFonteId(Guid valor) =>
    Validacao.Identificador(valor, nameof(FonteId));

  private static string? ValidarIdentificadorExterno(string? valor) =>
    valor is null ? null : Validacao.Texto(valor, nameof(IdentificadorExterno), 250);

  private static string ValidarTituloExterno(string valor) =>
    Validacao.Texto(valor, nameof(TituloExterno), 500);

  private static string ValidarUrl(string valor) =>
    Validacao.Url(valor, nameof(Url));

  private static string ValidarUrlHash(string valor) =>
    Validacao.Hash(valor, nameof(UrlHash));

  private static decimal ValidarPrecoAtual(decimal valor) =>
    Validacao.Numero(valor, nameof(PrecoAtual), 0.01m, 999999999999.99m, 2);

  private static decimal? ValidarPrecoPix(decimal? valor) =>
    valor is null ? null : Validacao.Numero(valor.Value, nameof(PrecoPix), 0.01m, 999999999999.99m, 2);

  private static decimal? ValidarPrecoAnterior(decimal? valor) =>
    valor is null ? null : Validacao.Numero(valor.Value, nameof(PrecoAnterior), 0.01m, 999999999999.99m, 2);

  private static short? ValidarQuantidadeParcelas(short? valor) =>
    valor is null ? null : (short)Validacao.Numero(valor.Value, nameof(QuantidadeParcelas), 1m, 32767m, 0);

  private static decimal? ValidarValorParcela(decimal? valor) =>
    valor is null ? null : Validacao.Numero(valor.Value, nameof(ValorParcela), 0.01m, 999999999999.99m, 2);

  private static string? ValidarCondicaoPreco(string? valor) =>
    valor is null ? null : Validacao.Texto(valor, nameof(CondicaoPreco), 300);

  private static DisponibilidadeOferta ValidarDisponibilidade(DisponibilidadeOferta valor) =>
    Validacao.Enumeracao(valor, nameof(Disponibilidade));

  private static CondicaoProduto ValidarCondicaoProduto(CondicaoProduto valor) =>
    Validacao.Enumeracao(valor, nameof(CondicaoProduto));

  private static string? ValidarImagemUrl(string? valor) =>
    valor is null ? null : Validacao.Url(valor, nameof(ImagemUrl));

  private static DateTimeOffset ValidarObservadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(ObservadaEm));

  private static DateTimeOffset ValidarValidaAte(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(ValidaAte));

  private static DateTimeOffset ValidarUltimaConfirmacaoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(UltimaConfirmacaoEm));

  private static StatusOferta ValidarStatus(StatusOferta valor) =>
    Validacao.Enumeracao(valor, nameof(Status));

  private static DateTimeOffset ValidarCriadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadaEm));

  private static DateTimeOffset ValidarAtualizadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(AtualizadaEm));
}

