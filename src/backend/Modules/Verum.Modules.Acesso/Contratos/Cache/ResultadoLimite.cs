namespace Verum.Modules.Acesso.Contratos.Cache;

public sealed record ResultadoLimite(bool Permitido, long Utilizado, long Restante, TimeSpan TentarNovamenteEm);
