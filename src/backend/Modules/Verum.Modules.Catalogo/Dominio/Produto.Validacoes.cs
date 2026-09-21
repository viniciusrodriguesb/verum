namespace Verum.Modules.Catalogo.Dominio;

internal sealed partial class Produto
{
  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static Guid ValidarCategoriaId(Guid valor) =>
    Validacao.Identificador(valor, nameof(CategoriaId));

  private static Guid ValidarMarcaId(Guid valor) =>
    Validacao.Identificador(valor, nameof(MarcaId));

  private static string ValidarNome(string valor) =>
    Validacao.Texto(valor, nameof(Nome), 250);

  private static string ValidarNomeNormalizado(string valor) =>
    Validacao.Texto(valor, nameof(NomeNormalizado), 250);

  private static string? ValidarModelo(string? valor) =>
    valor is null ? null : Validacao.Texto(valor, nameof(Modelo), 120);

  private static string ValidarSlug(string valor) =>
    Validacao.Texto(valor, nameof(Slug), 280);

  private static string? ValidarDescricao(string? valor) =>
    valor is null ? null : Validacao.Texto(valor, nameof(Descricao), int.MaxValue);

  private static string ValidarAtributos(string valor) =>
    Validacao.Json(valor, nameof(Atributos));

  private static StatusProduto ValidarStatus(StatusProduto valor) =>
    Validacao.Enumeracao(valor, nameof(Status));

  private static DateTimeOffset ValidarCriadoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadoEm));

  private static DateTimeOffset ValidarAtualizadoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(AtualizadoEm));
}

