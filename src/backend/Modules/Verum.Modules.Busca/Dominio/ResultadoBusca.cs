namespace Verum.Modules.Busca.Dominio;

internal sealed partial class ResultadoBusca
{
  public Guid Id { get; private set; }
  public Guid BuscaId { get; private set; }
  public string VersaoRanking { get; private set; } = null!;
  public int QuantidadeAnalisada { get; private set; }
  public int QuantidadeExibida { get; private set; }
  public bool Parcial { get; private set; }
  public DateTimeOffset GeradoEm { get; private set; }

  // Materialização pelo EF Core.
  private ResultadoBusca() { }

  public ResultadoBusca(
    Guid buscaId,
    string versaoRanking,
    int quantidadeAnalisada,
    int quantidadeExibida,
    bool parcial)
  {
    var agora = DateTimeOffset.UtcNow;
    Id = ValidarId(Guid.CreateVersion7());
    BuscaId = ValidarBuscaId(buscaId);
    VersaoRanking = ValidarVersaoRanking(versaoRanking);
    QuantidadeAnalisada = ValidarQuantidadeAnalisada(quantidadeAnalisada);
    QuantidadeExibida = ValidarQuantidadeExibida(quantidadeExibida);
    Parcial = ValidarParcial(parcial);
    GeradoEm = ValidarGeradoEm(agora);
    if (!(QuantidadeExibida <= QuantidadeAnalisada)) throw new ArgumentException("QuantidadeExibida não pode exceder QuantidadeAnalisada.");
  }
}

