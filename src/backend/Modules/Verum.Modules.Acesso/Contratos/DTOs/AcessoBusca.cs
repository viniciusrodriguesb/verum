namespace Verum.Modules.Acesso.Contratos.DTOs;

public sealed record AcessoBusca(string Contexto, string Pacote, int Limite, int Utilizado, int Restante,
  DateTimeOffset ReiniciaEm, DateTimeOffset? ConcessaoValidaAte);
