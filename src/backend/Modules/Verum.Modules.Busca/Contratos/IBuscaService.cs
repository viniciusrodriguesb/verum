namespace Verum.Modules.Busca.Contratos;

public sealed record SolicitarBusca(string Consulta, string ChaveIdempotencia);

public sealed record BuscaAceita(Guid Id, string Status);

public sealed record OfertaResultado(
  Guid OfertaId, Guid VarianteId, string Nome, string Url, decimal Preco, string CondicaoPreco,
  string? ImagemUrl, string Fonte, DateTimeOffset ObservadaEm, DateTimeOffset ValidaAte,
  int Posicao, decimal Pontuacao, string Explicacao)
{
  public decimal PontuacaoPreco { get; init; }

  public decimal PontuacaoConfianca { get; init; }

  public decimal PontuacaoAtualizacao { get; init; }
}

public sealed record BuscaConsulta(
  Guid Id, string Status, string Etapa, bool Parcial, string? Motivo, string? CodigoErro,
  int FontesConsultadas, int FontesComSucesso, int FontesComFalha,
  IReadOnlyList<OfertaResultado> Ofertas);

public interface IBuscaService
{
  Task<BuscaAceita> SolicitarAsync(Guid visitanteId, SolicitarBusca pedido, CancellationToken cancellationToken);

  Task<BuscaConsulta> ObterAsync(Guid visitanteId, Guid buscaId, CancellationToken cancellationToken);
}

public interface IProcessamentoBuscaService
{
  Task ProcessarAsync(Guid buscaId, CancellationToken cancellationToken);
}

public interface IFalhaBuscaService
{
  Task RegistrarAsync(Guid buscaId, CancellationToken cancellationToken);
}
