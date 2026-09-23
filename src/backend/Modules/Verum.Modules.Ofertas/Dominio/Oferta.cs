namespace Verum.Modules.Ofertas.Dominio;

internal sealed class Oferta
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


  public Loja Loja { get; private set; } = null!;

  public FonteOferta Fonte { get; private set; } = null!;

  private readonly List<OfertaObservacao> _observacoes = [];

  public IReadOnlyCollection<OfertaObservacao> Observacoes => _observacoes.AsReadOnly();

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
