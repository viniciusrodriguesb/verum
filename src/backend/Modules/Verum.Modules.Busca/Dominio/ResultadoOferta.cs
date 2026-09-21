namespace Verum.Modules.Busca.Dominio;

internal sealed partial class ResultadoOferta
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
}

