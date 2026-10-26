namespace Verum.Modules.Busca.Contratos.Configuracoes;

// Critérios versionados do algoritmo; alterações exigem revisar a versão e os fundamentos persistidos.
public static class CriteriosRanking
{
  public const decimal PesoPreco = 0.45m;

  public const decimal PesoConfianca = 0.35m;

  public const decimal PesoAtualizacao = 0.20m;

  public const decimal PesoConfiancaLoja = 0.7m;

  public const decimal PesoConfiancaDado = 0.3m;

  public const int JanelaHistoricoDias = 30;

  public const int MeiaVidaMinutos = 60;

  public const decimal ToleranciaPreco = 0.005m;

  public const int MinimoLojasMercado = 3;

  public const int MinimoObservacoesHistorico = 10;

  public const int MinimoLojasHistorico = 3;

  public const int ObservacoesConfiancaAlta = 30;

  public const int DiasConfiancaAlta = 14;

  public const int LojasConfiancaAlta = 5;

  public const decimal PesoHistorico = 0.6m;

  public const decimal PesoMercado = 0.4m;

  public const decimal NotaProvisoria = 50m;
}
