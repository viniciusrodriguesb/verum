namespace Verum.Modules.Notificacoes.Contratos.DTOs;

public sealed record PreferenciasAviso(bool EmailHabilitado, int SilencioInicio, int SilencioFim, string Fuso);
