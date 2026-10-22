namespace Verum.Modules.Notificacoes.Contratos.DTOs;

public sealed record NotificacaoConsulta(Guid Id, string Titulo, string Mensagem, DateTimeOffset CriadaEm,
  string? Url, IReadOnlyList<EntregaConsulta> Entregas);
