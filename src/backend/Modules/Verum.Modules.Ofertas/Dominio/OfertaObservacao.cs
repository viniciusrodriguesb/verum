namespace Verum.Modules.Ofertas.Dominio;

internal sealed partial class OfertaObservacao
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
}

