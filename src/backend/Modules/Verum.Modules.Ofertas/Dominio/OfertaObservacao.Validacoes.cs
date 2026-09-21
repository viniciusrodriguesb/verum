namespace Verum.Modules.Ofertas.Dominio;

internal sealed partial class OfertaObservacao
{
  private static long ValidarId(long valor) =>
    (long)Validacao.Numero(valor, nameof(Id), 0m, long.MaxValue, 0);

  private static Guid ValidarOfertaId(Guid valor) =>
    Validacao.Identificador(valor, nameof(OfertaId));

  private static Guid ValidarFonteId(Guid valor) =>
    Validacao.Identificador(valor, nameof(FonteId));

  private static decimal ValidarPreco(decimal valor) =>
    Validacao.Numero(valor, nameof(Preco), 0.01m, 999999999999.99m, 2);

  private static decimal? ValidarPrecoPix(decimal? valor) =>
    valor is null ? null : Validacao.Numero(valor.Value, nameof(PrecoPix), 0.01m, 999999999999.99m, 2);

  private static short? ValidarQuantidadeParcelas(short? valor) =>
    valor is null ? null : (short)Validacao.Numero(valor.Value, nameof(QuantidadeParcelas), 1m, 32767m, 0);

  private static decimal? ValidarValorParcela(decimal? valor) =>
    valor is null ? null : Validacao.Numero(valor.Value, nameof(ValorParcela), 0.01m, 999999999999.99m, 2);

  private static string? ValidarCondicaoPreco(string? valor) =>
    valor is null ? null : Validacao.Texto(valor, nameof(CondicaoPreco), 300);

  private static DisponibilidadeOferta ValidarDisponibilidade(DisponibilidadeOferta valor) =>
    Validacao.Enumeracao(valor, nameof(Disponibilidade));

  private static DateTimeOffset ValidarObservadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(ObservadaEm));

  private static string ValidarHashConteudo(string valor) =>
    Validacao.Hash(valor, nameof(HashConteudo));

  private static string? ValidarEvidencia(string? valor) =>
    valor is null ? null : Validacao.Json(valor, nameof(Evidencia));
}

