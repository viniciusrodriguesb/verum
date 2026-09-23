namespace Verum.Modules.Acesso.Contratos.Cache;

// Limite operacional rápido; acesso.registro_uso permanece a fonte oficial do consumo.
public sealed record ResultadoLimite(bool Permitido, long Utilizado, long Restante, TimeSpan TentarNovamenteEm);
