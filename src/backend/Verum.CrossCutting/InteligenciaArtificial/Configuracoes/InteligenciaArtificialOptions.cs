namespace Verum.CrossCutting.InteligenciaArtificial.Configuracoes;

public sealed class InteligenciaArtificialOptions
{
  public bool Enabled { get; set; }

  public string ProvedorPadrao { get; set; } = "OpenAI";
}
