using System.Text.Json;

namespace Verum.BuildingBlocks.InteligenciaArtificial;

public sealed record ConsultaIa(
  string Contexto,
  string Entrada,
  string NomeFormato,
  JsonElement Esquema,
  bool PesquisarWeb = false,
  string? Provedor = null);

public sealed record FonteIa(string Url, string? Titulo);

public sealed record RespostaIa(
  JsonElement Conteudo,
  string Provedor,
  string Modelo,
  string IdResposta,
  DateTimeOffset RecebidaEm,
  long? TokensEntrada,
  long? TokensSaida,
  IReadOnlyList<FonteIa> Fontes);

