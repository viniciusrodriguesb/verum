namespace Verum.Modules.Notificacoes.Contratos;

public interface IEnvioEmail
{
  Task EnviarAsync(Guid id, string destinatario, string assunto, string texto, CancellationToken cancellationToken);
}
