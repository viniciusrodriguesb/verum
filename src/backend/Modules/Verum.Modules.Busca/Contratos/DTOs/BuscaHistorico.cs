namespace Verum.Modules.Busca.Contratos.DTOs;

public sealed record BuscaHistorico(Guid Id, string Consulta, string Status, DateTimeOffset IniciadaEm, DateTimeOffset? ConcluidaEm);
