namespace Verum.Modules.Assinaturas.Dominio;

internal sealed partial class Plano
{
  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static string ValidarCodigo(string valor) =>
    Validacao.Texto(valor, nameof(Codigo), 80);

  private static string ValidarNome(string valor) =>
    Validacao.Texto(valor, nameof(Nome), 120);

  private static string ValidarDescricao(string valor) =>
    Validacao.Texto(valor, nameof(Descricao), 500);

  private static string ValidarPacoteAcessoCodigo(string valor) =>
    Validacao.Texto(valor, nameof(PacoteAcessoCodigo), 80);

  private static decimal ValidarValorAtual(decimal valor) =>
    Validacao.Numero(valor, nameof(ValorAtual), 0.01m, 999999999999.99m, 2);

  private static string ValidarMoeda(string valor) =>
    Validacao.Moeda(valor, nameof(Moeda));

  private static PeriodicidadePlano ValidarPeriodicidade(PeriodicidadePlano valor) =>
    Validacao.Enumeracao(valor, nameof(Periodicidade));

  private static bool ValidarAtivo(bool valor) =>
    valor;

  private static DateTimeOffset ValidarCriadoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadoEm));

  private static DateTimeOffset ValidarAtualizadoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(AtualizadoEm));
}

