namespace Verum.Modules.Ofertas.Contratos.Descoberta;

public interface IConsultaLoja
{
  Task<ResultadoConsultaLoja> ConsultarAsync(string loja, string consulta, CancellationToken cancellationToken = default);
}

public sealed record ItemConsultaLoja(string Url, IReadOnlyDictionary<string, string?> Campos);

public sealed record ItemLojaDescartado(int Pagina, int Indice, string Codigo);

public sealed record ResultadoConsultaLoja(
  string Loja,
  DateTimeOffset ConsultadaEm,
  IReadOnlyList<ItemConsultaLoja> Itens,
  IReadOnlyList<ItemLojaDescartado> Descartados,
  bool LimiteAtingido);

