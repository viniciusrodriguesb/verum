namespace Verum.Modules.Notificacoes.Aplicacao.Configuracoes;

public sealed class EmailOptions
{
  public bool Habilitado { get; set; }

  public string Host { get; set; } = "";

  public int Porta { get; set; } = 587;

  public bool Tls { get; set; } = true;

  public string Usuario { get; set; } = "";

  public string Senha { get; set; } = "";

  public string Remetente { get; set; } = "";

  public int MaximoTentativas { get; set; } = 5;
}
