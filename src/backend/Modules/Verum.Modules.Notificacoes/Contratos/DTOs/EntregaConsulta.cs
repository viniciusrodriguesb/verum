namespace Verum.Modules.Notificacoes.Contratos.DTOs;

public sealed record EntregaConsulta(string Estado, short QuantidadeTentativas, string? CodigoErro);
