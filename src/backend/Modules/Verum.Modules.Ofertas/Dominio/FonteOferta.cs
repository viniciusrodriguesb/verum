namespace Verum.Modules.Ofertas.Dominio;

internal sealed partial class FonteOferta
{
  public Guid Id { get; private set; }
  public string Nome { get; private set; } = null!;
  public TipoFonte Tipo { get; private set; }
  public string Codigo { get; private set; } = null!;
  public bool Ativa { get; private set; }
  public decimal NivelConfianca { get; private set; }
  public DateTimeOffset CriadaEm { get; private set; }

  // Materialização pelo EF Core.
  private FonteOferta() { }

  public FonteOferta(
    string nome,
    TipoFonte tipo,
    string codigo,
    decimal nivelConfianca)
  {
    var agora = DateTimeOffset.UtcNow;
    Id = ValidarId(Guid.CreateVersion7());
    Nome = ValidarNome(nome);
    Tipo = ValidarTipo(tipo);
    Codigo = ValidarCodigo(codigo);
    Ativa = ValidarAtiva(true);
    NivelConfianca = ValidarNivelConfianca(nivelConfianca);
    CriadaEm = ValidarCriadaEm(agora);

  }
}

