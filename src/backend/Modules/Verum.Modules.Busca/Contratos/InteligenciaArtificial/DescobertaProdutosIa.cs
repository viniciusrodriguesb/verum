using Verum.BuildingBlocks.InteligenciaArtificial;

namespace Verum.Modules.Busca.Contratos.InteligenciaArtificial;

public sealed record DescobertaProdutosIa(ResultadoProdutosIa Resultado, RespostaIa Execucao);

public sealed record ResultadoProdutosIa
{
  public required string ConsultaInterpretada { get; init; }

  public required string? MotivoSemResultado { get; init; }

  public required ProdutoCandidatoIa[] Produtos { get; init; }
}

public sealed record ProdutoCandidatoIa
{
  public required string Nome { get; init; }

  public required string? Marca { get; init; }

  public required string? Modelo { get; init; }

  public required string? Variante { get; init; }

  public required string? Gtin { get; init; }

  public required string Loja { get; init; }

  public required string? Vendedor { get; init; }

  public required string Url { get; init; }

  public required string? ImagemUrl { get; init; }

  public required decimal? Preco { get; init; }

  public required decimal? PrecoPix { get; init; }

  public required decimal? PrecoAnterior { get; init; }

  public required int? QuantidadeParcelas { get; init; }

  public required decimal? ValorParcela { get; init; }

  public required string? CondicaoPreco { get; init; }

  public required string? Moeda { get; init; }

  public required string Disponibilidade { get; init; }

  public required string CondicaoProduto { get; init; }

  public required string Correspondencia { get; init; }

  public required string Evidencia { get; init; }
}

