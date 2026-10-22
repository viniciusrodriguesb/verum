using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Verum.Modules.Notificacoes.Aplicacao.Configuracoes;
using Verum.Modules.Notificacoes.Infraestrutura.Email;
using Xunit;

namespace Verum.Tests.Integracao;

public sealed class EmailRadarTests
{
  [MailpitFact]
  public async Task SmtpEntregaMensagemRastreavelNoMailpitLocal()
  {
    var id = Guid.NewGuid();

    var assunto = "VerumRadarTest" + id.ToString("N");

    var envio = new EnvioEmailSmtp(Options.Create(new EmailOptions
    {
      Habilitado = true,
      Host = "localhost",
      Porta = 1025,
      Tls = false,
      Remetente = "radar@verum.local"
    }));

    using var prazo = new CancellationTokenSource(TimeSpan.FromSeconds(15));

    await envio.EnviarAsync(id, "radar-test@example.test", assunto,
      "Meta atingida. https://loja.example/produto — frete não incluído.", prazo.Token);

    using var http = new HttpClient { BaseAddress = new Uri("http://localhost:8025") };

    var resultado = await http.GetFromJsonAsync<JsonElement>("/api/v1/search?query=" + Uri.EscapeDataString("subject:" + assunto), prazo.Token);

    var mensagem = Assert.Single(resultado.GetProperty("messages").EnumerateArray());

    Assert.Equal(assunto, mensagem.GetProperty("Subject").GetString());

    // Remove somente a mensagem sintética criada por este teste.
    using var apagar = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/messages")
    {
      Content = JsonContent.Create(new { IDs = new[] { mensagem.GetProperty("ID").GetString() } })
    };

    (await http.SendAsync(apagar, prazo.Token)).EnsureSuccessStatusCode();
  }
}

public sealed class MailpitFactAttribute : FactAttribute
{
  public MailpitFactAttribute()
  {
    if (Environment.GetEnvironmentVariable("VERUM_TEST_MAILPIT") != "true")
      Skip = "Configure VERUM_TEST_MAILPIT=true para validar SMTP e a API do Mailpit local.";
  }
}
