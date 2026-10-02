namespace Verum.Modules.Acesso.Contratos.Cache.DTOs;

public sealed record ResultadoLimite(bool Permitido, long Utilizado, long Restante, TimeSpan TentarNovamenteEm);
