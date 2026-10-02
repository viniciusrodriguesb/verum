using System.Text.Json;

namespace Verum.BuildingBlocks.InteligenciaArtificial.DTOs;

public sealed record ConsultaIa(
  string Contexto,
  string Entrada,
  string NomeFormato,
  JsonElement Esquema,
  bool PesquisarWeb = false,
  string? Provedor = null);
