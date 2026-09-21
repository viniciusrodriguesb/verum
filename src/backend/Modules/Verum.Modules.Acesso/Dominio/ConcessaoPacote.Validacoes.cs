namespace Verum.Modules.Acesso.Dominio;

internal sealed partial class ConcessaoPacote
{
  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static Guid ValidarContaId(Guid valor) =>
    Validacao.Identificador(valor, nameof(ContaId));

  private static Guid ValidarPacoteAcessoId(Guid valor) =>
    Validacao.Identificador(valor, nameof(PacoteAcessoId));

  private static OrigemConcessao ValidarOrigem(OrigemConcessao valor) =>
    Validacao.Enumeracao(valor, nameof(Origem));

  private static Guid? ValidarReferenciaOrigemId(Guid? valor) =>
    valor is null ? null : Validacao.Identificador(valor.Value, nameof(ReferenciaOrigemId));

  private static DateTimeOffset ValidarValidaDe(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(ValidaDe));

  private static DateTimeOffset? ValidarValidaAte(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(ValidaAte));

  private static DateTimeOffset? ValidarRevogadaEm(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(RevogadaEm));

  private static DateTimeOffset ValidarCriadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadaEm));
}

