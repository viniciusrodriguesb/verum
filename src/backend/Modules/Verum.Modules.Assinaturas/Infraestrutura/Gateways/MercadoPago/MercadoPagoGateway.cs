using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Verum.BuildingBlocks.Erros;
using Verum.Modules.Assinaturas.Aplicacao.Gateways;
using Verum.Modules.Assinaturas.Aplicacao.Gateways.DTOs;
using Verum.Modules.Assinaturas.Infraestrutura.Gateways.MercadoPago.Configuracoes;
using Verum.Modules.Assinaturas.Infraestrutura.Gateways.MercadoPago.DTOs;

namespace Verum.Modules.Assinaturas.Infraestrutura.Gateways.MercadoPago;

internal sealed class MercadoPagoGateway(IHttpClientFactory clients, IOptions<MercadoPagoOptions> options) : IGatewayAssinaturas
{
  private static readonly JsonSerializerOptions Json = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,

    NumberHandling = JsonNumberHandling.AllowReadingFromString
  };

  public string Nome => "MercadoPago";

  public bool ValidarWebhook(string recursoId, string assinatura, string requestId)
  {
    if (string.IsNullOrWhiteSpace(options.Value.WebhookSecret) || requestId.Length is 0 or > 100
      || recursoId.Length is 0 or > 100 || assinatura.Length > 500) return false;

    var partes = assinatura.Split(',').Select(x => x.Trim().Split('=', 2)).Where(x => x.Length == 2).ToArray();

    var timestamps = partes.Where(x => x[0] == "ts").Select(x => x[1]).ToArray();

    var hashes = partes.Where(x => x[0] == "v1").Select(x => x[1]).ToArray();

    if (timestamps.Length != 1 || !long.TryParse(timestamps[0], out _)) return false;

    var manifesto = $"id:{recursoId.ToLowerInvariant()};request-id:{requestId};ts:{timestamps[0]};";

    var esperado = HMACSHA256.HashData(Encoding.UTF8.GetBytes(options.Value.WebhookSecret), Encoding.UTF8.GetBytes(manifesto));

    // A deduplicação persistente protege os reenvios; não rejeitamos notificações legítimas atrasadas por idade do ts.
    return hashes.Any(hash => hash.Length == 64 && hash.All(Uri.IsHexDigit)
      && CryptographicOperations.FixedTimeEquals(Convert.FromHexString(hash), esperado));
  }

  public async Task<AssinaturaGateway> CriarAsync(Guid referencia, string email, string plano, decimal valor, CancellationToken cancellationToken)
  {
    var resposta = await EnviarAsync<PreapprovalResposta>(HttpMethod.Post, "preapproval", new
    {
      reason = $"Verum {plano} - {referencia:D}",
      external_reference = referencia.ToString(),
      payer_email = email,
      auto_recurring = new { frequency = 1, frequency_type = "months", transaction_amount = valor, currency_id = "BRL" },
      back_url = options.Value.UrlRetorno,
      notification_url = options.Value.UrlWebhook,
      status = "pending"
    }, cancellationToken);

    return Converter(resposta);
  }

  public async Task<AssinaturaGateway?> LocalizarAsync(Guid referencia, CancellationToken cancellationToken)
  {
    var encontradas = new List<PreapprovalResposta>();

    // q é o filtro documentado da API; a comparação exata evita associar outro resultado da busca livre.
    await foreach (var resultado in PesquisarAsync<PreapprovalResposta>($"preapproval/search?q={referencia:D}", cancellationToken))
      if (resultado.ExternalReference == referencia.ToString()) encontradas.Add(resultado);

    if (encontradas.Count > 1) throw Invalido();

    return encontradas.Count == 0 ? null : Converter(encontradas[0]);
  }

  public async Task<AssinaturaGateway> ObterAsync(string id, CancellationToken cancellationToken) =>
    Converter(await EnviarAsync<PreapprovalResposta>(HttpMethod.Get, "preapproval/" + Uri.EscapeDataString(id), null, cancellationToken));

  public async Task CancelarAsync(string id, CancellationToken cancellationToken) =>
    _ = await EnviarAsync<PreapprovalResposta>(HttpMethod.Put, "preapproval/" + Uri.EscapeDataString(id),
      new { status = "cancelled" }, cancellationToken);

  public async Task<string?> ResolverAssinaturaAsync(string tipo, string recursoId, CancellationToken cancellationToken)
  {
    if (tipo == "subscription_preapproval") return recursoId;

    if (tipo == "subscription_authorized_payment")
      return (await EnviarAsync<FaturaResposta>(HttpMethod.Get, "authorized_payments/" + Uri.EscapeDataString(recursoId), null, cancellationToken)).PreapprovalId;

    var faturas = await EnviarAsync<PesquisaResposta<FaturaResposta>>(HttpMethod.Get,
      "authorized_payments/search?payment_id=" + Uri.EscapeDataString(recursoId), null, cancellationToken);

    return faturas.Results.SingleOrDefault(x => x.Payment?.Id?.ToString(System.Globalization.CultureInfo.InvariantCulture) == recursoId)?.PreapprovalId;
  }

  public async Task<IReadOnlyList<CobrancaGateway>> ConsultarCobrancasAsync(string assinaturaId, CancellationToken cancellationToken)
  {
    var cobrancas = new List<CobrancaGateway>();

    await foreach (var fatura in PesquisarAsync<FaturaResposta>(
      $"authorized_payments/search?preapproval_id={Uri.EscapeDataString(assinaturaId)}", cancellationToken))
    {
      if (fatura.Payment?.Id is not long id) continue;

      var pagamento = await ObterPagamentoAsync(id.ToString(System.Globalization.CultureInfo.InvariantCulture), cancellationToken);

      if (fatura.PreapprovalId != assinaturaId || !Guid.TryParse(fatura.ExternalReference, out var referencia)
        || (!string.IsNullOrEmpty(pagamento.ExternalReference) && pagamento.ExternalReference != referencia.ToString())
        || fatura.TransactionAmount != pagamento.TransactionAmount
        || fatura.CurrencyId != pagamento.CurrencyId) throw Invalido();

      var status = pagamento.TransactionAmountRefunded > 0 ? "refunded" : pagamento.Status;

      cobrancas.Add(new(id.ToString(System.Globalization.CultureInfo.InvariantCulture), assinaturaId, referencia, status,
        pagamento.TransactionAmount, pagamento.CurrencyId, pagamento.DateApproved,
        (fatura.DebitDate ?? pagamento.DateApproved ?? pagamento.DateCreated).ToUniversalTime(),
        (pagamento.DateLastUpdated ?? pagamento.DateCreated).ToUniversalTime()));
    }

    return cobrancas;
  }

  private async IAsyncEnumerable<T> PesquisarAsync<T>(string caminho, [EnumeratorCancellation] CancellationToken cancellationToken)
  {
    var offset = 0;

    while (true)
    {
      var pagina = await EnviarAsync<PesquisaResposta<T>>(HttpMethod.Get,
        $"{caminho}&limit=50&offset={offset}", null, cancellationToken);

      foreach (var resultado in pagina.Results) yield return resultado;

      offset += pagina.Results.Count;

      if (pagina.Results.Count == 0 || (pagina.Paging is null ? pagina.Results.Count < 50 : offset >= pagina.Paging.Total)) yield break;
    }
  }

  private async Task<PagamentoResposta> ObterPagamentoAsync(string id, CancellationToken cancellationToken)
  {
    var pagamento = await EnviarAsync<PagamentoResposta>(HttpMethod.Get, "v1/payments/" + Uri.EscapeDataString(id), null, cancellationToken);

    if (pagamento.CollectorId != options.Value.CollectorId || pagamento.LiveMode != options.Value.Producao
      || pagamento.Id.ToString(System.Globalization.CultureInfo.InvariantCulture) != id) throw Invalido();

    return pagamento;
  }

  private AssinaturaGateway Converter(PreapprovalResposta resposta)
  {
    if (resposta.CollectorId != options.Value.CollectorId || !Guid.TryParse(resposta.ExternalReference, out var referencia)
      || resposta.AutoRecurring.Frequency != 1 || resposta.AutoRecurring.FrequencyType != "months") throw Invalido();

    if (resposta.InitPoint is string url && (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
      || uri.Scheme != "https" || uri.Host != "www.mercadopago.com.br")) throw Invalido();

    return new(resposta.Id, referencia, resposta.Status, resposta.AutoRecurring.TransactionAmount,
      resposta.AutoRecurring.CurrencyId, resposta.InitPoint);
  }

  private async Task<T> EnviarAsync<T>(HttpMethod metodo, string caminho, object? corpo, CancellationToken cancellationToken)
  {
    using var client = clients.CreateClient("MercadoPago");

    using var request = new HttpRequestMessage(metodo, new Uri(new Uri("https://api.mercadopago.com/"), caminho));

    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.AccessToken);

    if (corpo is not null) request.Content = JsonContent.Create(corpo);

    // Sem retry automático em POST/PUT: timeout de criação é ambíguo e é recuperado pela external_reference.
    using var response = await client.SendAsync(request, cancellationToken);

    if (!response.IsSuccessStatusCode)
      throw new ErroAplicacaoException("GATEWAY_INDISPONIVEL", "Não foi possível confirmar a operação com o gateway.", TipoErro.Indisponivel);

    return await response.Content.ReadFromJsonAsync<T>(Json, cancellationToken) ?? throw Invalido();
  }

  private static ErroAplicacaoException Invalido() =>
    new("GATEWAY_RESPOSTA_INVALIDA", "O gateway retornou dados incompatíveis com a assinatura.", TipoErro.Conflito);
}
