using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Verum.Modules.Notificacoes.Aplicacao.Configuracoes;
using Verum.Modules.Notificacoes.Contratos;

namespace Verum.Modules.Notificacoes.Infraestrutura.Email;

internal sealed class EnvioEmailSmtp(IOptions<EmailOptions> options) : IEnvioEmail
{
  public async Task EnviarAsync(Guid id, string destinatario, string assunto, string texto, CancellationToken cancellationToken)
  {
    var settings = options.Value;

    var mensagem = new MimeMessage();

    mensagem.From.Add(new MailboxAddress("Verum Radar", settings.Remetente));

    mensagem.To.Add(MailboxAddress.Parse(destinatario));

    mensagem.MessageId = id.ToString("N") + "@" + mensagem.From.Mailboxes.Single().Domain;

    mensagem.Subject = assunto;

    mensagem.Body = new TextPart("plain") { Text = texto };

    using var cliente = new SmtpClient { Timeout = 15000 };

    await cliente.ConnectAsync(settings.Host, settings.Porta,
      settings.Tls ? SecureSocketOptions.StartTls : SecureSocketOptions.None, cancellationToken);

    if (!string.IsNullOrWhiteSpace(settings.Usuario))
      await cliente.AuthenticateAsync(settings.Usuario, settings.Senha, cancellationToken);

    await cliente.SendAsync(mensagem, cancellationToken);

    await cliente.DisconnectAsync(true, cancellationToken);
  }
}
