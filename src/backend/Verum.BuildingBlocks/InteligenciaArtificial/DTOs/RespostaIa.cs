using System.Text.Json;

namespace Verum.BuildingBlocks.InteligenciaArtificial.DTOs;

public sealed record RespostaIa(
  JsonElement Conteudo,
  string Provedor,
  string Modelo,
  string IdResposta,
  DateTimeOffset RecebidaEm,
  long? TokensEntrada,
  long? TokensSaida,
  IReadOnlyList<FonteIa> Fontes);
