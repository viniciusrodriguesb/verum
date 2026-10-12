namespace Verum.Modules.Contas.Contratos.DTOs;

public sealed record ContaAutenticada(Guid Id, string SujeitoIdentidade, string Email, string NomeExibicao);
