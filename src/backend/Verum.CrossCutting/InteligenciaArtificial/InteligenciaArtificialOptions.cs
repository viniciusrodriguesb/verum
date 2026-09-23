namespace Verum.CrossCutting.InteligenciaArtificial;

public sealed class InteligenciaArtificialOptions
{
  public bool Enabled { get; set; }

  public string ProvedorPadrao { get; set; } = "OpenAI";
}

public sealed class OpenAiOptions
{
  public string ApiKey { get; set; } = string.Empty;

  public string Modelo { get; set; } = "gpt-4.1-mini";

  public int MaxOutputTokens { get; set; } = 6000;

  public int MaxToolCalls { get; set; } = 3;
}

