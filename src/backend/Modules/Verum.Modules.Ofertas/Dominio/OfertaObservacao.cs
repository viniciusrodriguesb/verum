namespace Verum.Modules.Ofertas.Dominio;

internal sealed class OfertaObservacao
{
  public long Id { get; private set; }

  public Guid OfertaId { get; private set; }

  public Guid FonteId { get; private set; }

  public decimal Preco { get; private set; }

  public decimal? PrecoPix { get; private set; }

  public short? QuantidadeParcelas { get; private set; }

  public decimal? ValorParcela { get; private set; }

  public string? CondicaoPreco { get; private set; }

  public DisponibilidadeOferta Disponibilidade { get; private set; }

  public DateTimeOffset ObservadaEm { get; private set; }

  public string HashConteudo { get; private set; } = null!;

  public string? Evidencia { get; private set; }


  public Oferta Oferta { get; private set; } = null!;

  public FonteOferta Fonte { get; private set; } = null!;

  // Materialização pelo EF Core.
  private OfertaObservacao() { }

  public OfertaObservacao(
    Guid ofertaId,
    Guid fonteId,
    decimal preco,
    DisponibilidadeOferta disponibilidade,
    DateTimeOffset observadaEm,
    string hashConteudo,
    decimal? precoPix = null,
    short? quantidadeParcelas = null,
    decimal? valorParcela = null,
    string? condicaoPreco = null,
    string? evidencia = null)
  {
    Id = ValidarId(0);

    OfertaId = ValidarOfertaId(ofertaId);

    FonteId = ValidarFonteId(fonteId);

    Preco = ValidarPreco(preco);

    PrecoPix = ValidarPrecoPix(precoPix);

    QuantidadeParcelas = ValidarQuantidadeParcelas(quantidadeParcelas);

    ValorParcela = ValidarValorParcela(valorParcela);

    CondicaoPreco = ValidarCondicaoPreco(condicaoPreco);

    Disponibilidade = ValidarDisponibilidade(disponibilidade);

    ObservadaEm = ValidarObservadaEm(observadaEm);

    HashConteudo = ValidarHashConteudo(hashConteudo);

    Evidencia = ValidarEvidencia(evidencia);

    if (!(QuantidadeParcelas.HasValue == ValorParcela.HasValue)) throw new ArgumentException("QuantidadeParcelas e ValorParcela devem ser informados juntos.");
  }

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
