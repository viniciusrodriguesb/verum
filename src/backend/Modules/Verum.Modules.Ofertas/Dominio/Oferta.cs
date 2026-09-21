namespace Verum.Modules.Ofertas.Dominio;

internal sealed partial class Oferta
{
  public Guid Id { get; private set; }
  public Guid ProdutoVarianteId { get; private set; }
  public Guid LojaId { get; private set; }
  public Guid FonteId { get; private set; }
  public string? IdentificadorExterno { get; private set; }
  public string TituloExterno { get; private set; } = null!;
  public string Url { get; private set; } = null!;
  public string UrlHash { get; private set; } = null!;
  public decimal PrecoAtual { get; private set; }
  public decimal? PrecoPix { get; private set; }
  public decimal? PrecoAnterior { get; private set; }
  public short? QuantidadeParcelas { get; private set; }
  public decimal? ValorParcela { get; private set; }
  public string? CondicaoPreco { get; private set; }
  public DisponibilidadeOferta Disponibilidade { get; private set; }
  public CondicaoProduto CondicaoProduto { get; private set; }
  public string? ImagemUrl { get; private set; }
  public DateTimeOffset ObservadaEm { get; private set; }
  public DateTimeOffset ValidaAte { get; private set; }
  public DateTimeOffset UltimaConfirmacaoEm { get; private set; }
  public StatusOferta Status { get; private set; }
  public DateTimeOffset CriadaEm { get; private set; }
  public DateTimeOffset AtualizadaEm { get; private set; }

  // Materialização pelo EF Core.
  private Oferta() { }

  public Oferta(
    Guid produtoVarianteId,
    Guid lojaId,
    Guid fonteId,
    string tituloExterno,
    string url,
    decimal precoAtual,
    DisponibilidadeOferta disponibilidade,
    CondicaoProduto condicaoProduto,
    DateTimeOffset observadaEm,
    DateTimeOffset validaAte,
    string? identificadorExterno = null,
    decimal? precoPix = null,
    decimal? precoAnterior = null,
    short? quantidadeParcelas = null,
    decimal? valorParcela = null,
    string? condicaoPreco = null,
    string? imagemUrl = null)
  {
    var agora = DateTimeOffset.UtcNow;
    Id = ValidarId(Guid.CreateVersion7());
    ProdutoVarianteId = ValidarProdutoVarianteId(produtoVarianteId);
    LojaId = ValidarLojaId(lojaId);
    FonteId = ValidarFonteId(fonteId);
    IdentificadorExterno = ValidarIdentificadorExterno(identificadorExterno);
    TituloExterno = ValidarTituloExterno(tituloExterno);
    Url = ValidarUrl(url);
    UrlHash = ValidarUrlHash(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(Url))).ToLowerInvariant());
    PrecoAtual = ValidarPrecoAtual(precoAtual);
    PrecoPix = ValidarPrecoPix(precoPix);
    PrecoAnterior = ValidarPrecoAnterior(precoAnterior);
    QuantidadeParcelas = ValidarQuantidadeParcelas(quantidadeParcelas);
    ValorParcela = ValidarValorParcela(valorParcela);
    CondicaoPreco = ValidarCondicaoPreco(condicaoPreco);
    Disponibilidade = ValidarDisponibilidade(disponibilidade);
    CondicaoProduto = ValidarCondicaoProduto(condicaoProduto);
    ImagemUrl = ValidarImagemUrl(imagemUrl);
    ObservadaEm = ValidarObservadaEm(observadaEm);
    ValidaAte = ValidarValidaAte(validaAte);
    UltimaConfirmacaoEm = ValidarUltimaConfirmacaoEm(ObservadaEm);
    Status = ValidarStatus(StatusOferta.Ativa);
    CriadaEm = ValidarCriadaEm(agora);
    AtualizadaEm = ValidarAtualizadaEm(agora);
    if (!(ValidaAte > ObservadaEm)) throw new ArgumentException("ValidaAte deve ser posterior à observação.");
    if (!(QuantidadeParcelas.HasValue == ValorParcela.HasValue)) throw new ArgumentException("QuantidadeParcelas e ValorParcela devem ser informados juntos.");
  }
}

