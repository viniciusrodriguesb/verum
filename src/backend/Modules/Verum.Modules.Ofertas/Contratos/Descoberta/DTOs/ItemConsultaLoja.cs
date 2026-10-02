namespace Verum.Modules.Ofertas.Contratos.Descoberta.DTOs;

public sealed record ItemConsultaLoja(string Url, IReadOnlyDictionary<string, string?> Campos);
