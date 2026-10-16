namespace Verum.Modules.Acesso.Aplicacao.Configuracoes;

public sealed class AcessoOptions
{
  public int LimiteVisitanteDiario { get; set; } = 1;

  public int LimiteFreeDiario { get; set; } = 3;

  public int LimitePagoDiario { get; set; } = 30;
}
