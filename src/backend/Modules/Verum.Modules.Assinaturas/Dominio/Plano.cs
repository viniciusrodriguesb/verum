namespace Verum.Modules.Assinaturas.Dominio;

internal sealed partial class Plano
{
  public Guid Id { get; private set; }
  public string Codigo { get; private set; } = null!;
  public string Nome { get; private set; } = null!;
  public string Descricao { get; private set; } = null!;
  public string PacoteAcessoCodigo { get; private set; } = null!;
  public decimal ValorAtual { get; private set; }
  public string Moeda { get; private set; } = null!;
  public PeriodicidadePlano Periodicidade { get; private set; }
  public bool Ativo { get; private set; }
  public DateTimeOffset CriadoEm { get; private set; }
  public DateTimeOffset AtualizadoEm { get; private set; }

  // Materialização pelo EF Core.
  private Plano() { }

  public Plano(
    string codigo,
    string nome,
    string descricao,
    string pacoteAcessoCodigo,
    decimal valorAtual,
    string moeda,
    PeriodicidadePlano periodicidade)
  {
    var agora = DateTimeOffset.UtcNow;
    Id = ValidarId(Guid.CreateVersion7());
    Codigo = ValidarCodigo(codigo);
    Nome = ValidarNome(nome);
    Descricao = ValidarDescricao(descricao);
    PacoteAcessoCodigo = ValidarPacoteAcessoCodigo(pacoteAcessoCodigo);
    ValorAtual = ValidarValorAtual(valorAtual);
    Moeda = ValidarMoeda(moeda);
    Periodicidade = ValidarPeriodicidade(periodicidade);
    Ativo = ValidarAtivo(true);
    CriadoEm = ValidarCriadoEm(agora);
    AtualizadoEm = ValidarAtualizadoEm(agora);

  }
}

