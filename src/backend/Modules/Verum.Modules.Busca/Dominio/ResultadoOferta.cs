namespace Verum.Modules.Busca.Dominio;

internal sealed class ResultadoOferta
{
  public long Id { get; private set; }

  public Guid ResultadoBuscaId { get; private set; }

  public Guid OfertaId { get; private set; }

  public Guid ProdutoVarianteId { get; private set; }

  public short Posicao { get; private set; }

  public ClassificacaoOferta Classificacao { get; private set; }

  public decimal PontuacaoFinal { get; private set; }

  public decimal PontuacaoPreco { get; private set; }

  public decimal PontuacaoConfianca { get; private set; }

  public decimal PontuacaoAtualizacao { get; private set; }

  public string Explicacao { get; private set; } = null!;

  public string OfertaSnapshot { get; private set; } = null!;


  public ResultadoBusca Resultado { get; private set; } = null!;

  // Materialização pelo EF Core.
  private ResultadoOferta() { }

  public ResultadoOferta(
    Guid resultadoBuscaId,
    Guid ofertaId,
    Guid produtoVarianteId,
    short posicao,
    ClassificacaoOferta classificacao,
    decimal pontuacaoFinal,
    decimal pontuacaoPreco,
    decimal pontuacaoConfianca,
    decimal pontuacaoAtualizacao,
    string explicacao,
    string ofertaSnapshot)
  {
    Id = ValidarId(0);

    ResultadoBuscaId = ValidarResultadoBuscaId(resultadoBuscaId);

    OfertaId = ValidarOfertaId(ofertaId);

    ProdutoVarianteId = ValidarProdutoVarianteId(produtoVarianteId);

    Posicao = ValidarPosicao(posicao);

    Classificacao = ValidarClassificacao(classificacao);

    PontuacaoFinal = ValidarPontuacaoFinal(pontuacaoFinal);

    PontuacaoPreco = ValidarPontuacaoPreco(pontuacaoPreco);

    PontuacaoConfianca = ValidarPontuacaoConfianca(pontuacaoConfianca);

    PontuacaoAtualizacao = ValidarPontuacaoAtualizacao(pontuacaoAtualizacao);

    Explicacao = ValidarExplicacao(explicacao);

    OfertaSnapshot = ValidarOfertaSnapshot(ofertaSnapshot);
  }

  private static long ValidarId(long valor) =>
    (long)Validacao.Numero(valor, nameof(Id), 0m, long.MaxValue, 0);

  private static Guid ValidarResultadoBuscaId(Guid valor) =>
    Validacao.Identificador(valor, nameof(ResultadoBuscaId));

  private static Guid ValidarOfertaId(Guid valor) =>
    Validacao.Identificador(valor, nameof(OfertaId));

  private static Guid ValidarProdutoVarianteId(Guid valor) =>
    Validacao.Identificador(valor, nameof(ProdutoVarianteId));

  private static short ValidarPosicao(short valor) =>
    (short)Validacao.Numero(valor, nameof(Posicao), 1m, 32767m, 0);

  private static ClassificacaoOferta ValidarClassificacao(ClassificacaoOferta valor) =>
    Validacao.Enumeracao(valor, nameof(Classificacao));

  private static decimal ValidarPontuacaoFinal(decimal valor) =>
    Validacao.Numero(valor, nameof(PontuacaoFinal), 0m, 100m, 2);

  private static decimal ValidarPontuacaoPreco(decimal valor) =>
    Validacao.Numero(valor, nameof(PontuacaoPreco), 0m, 100m, 2);

  private static decimal ValidarPontuacaoConfianca(decimal valor) =>
    Validacao.Numero(valor, nameof(PontuacaoConfianca), 0m, 100m, 2);

  private static decimal ValidarPontuacaoAtualizacao(decimal valor) =>
    Validacao.Numero(valor, nameof(PontuacaoAtualizacao), 0m, 100m, 2);

  private static string ValidarExplicacao(string valor) =>
    Validacao.Texto(valor, nameof(Explicacao), 1000);

  private static string ValidarOfertaSnapshot(string valor) =>
    Validacao.Json(valor, nameof(OfertaSnapshot));
}
