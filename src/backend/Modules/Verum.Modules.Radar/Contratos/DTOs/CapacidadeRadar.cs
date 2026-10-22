namespace Verum.Modules.Radar.Contratos.DTOs;

public sealed record CapacidadeRadar(bool Disponivel, bool Permitido, int Limite, int Utilizado,
  int IntervaloMinutos, bool EmailDisponivel, string PlanoNecessario = "VERUM_PLUS", bool VendaDisponivel = false);
