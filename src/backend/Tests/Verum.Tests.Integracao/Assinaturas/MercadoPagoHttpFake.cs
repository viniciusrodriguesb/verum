using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Verum.Tests.Integracao;

// Simula somente a rede do Mercado Pago. Persistência, transações, assinatura HMAC e regras usam as implementações reais.
internal sealed class MercadoPagoHttpFake : HttpMessageHandler
{
  public readonly ConcurrentDictionary<string, Estado> Assinaturas = new();

  public bool Indisponivel { get; set; }

  public bool PerderRespostaCriacao { get; set; }

  public bool ValorIncorreto { get; set; }

  public bool ColetorIncorreto { get; set; }

  public int Criacoes;

  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
  {
    if (Indisponivel) return new(HttpStatusCode.ServiceUnavailable);

    var path = request.RequestUri!.AbsolutePath;

    var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(request.RequestUri.Query);

    if (request.Method == HttpMethod.Post && path == "/preapproval")
    {
      using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));

      var id = "mp-" + Interlocked.Increment(ref Criacoes);

      var estado = new Estado
      {
        Id = id,
        Referencia = body.RootElement.GetProperty("external_reference").GetString()!,
        Valor = body.RootElement.GetProperty("auto_recurring").GetProperty("transaction_amount").GetDecimal()
      };

      Assinaturas[id] = estado;

      if (PerderRespostaCriacao) throw new HttpRequestException("Simula queda após o gateway criar a assinatura.");

      return Responder(Preapproval(estado));
    }

    if (path == "/preapproval/search")
      return Responder(new { results = Assinaturas.Values.Where(x => x.Referencia == query["q"]).Select(Preapproval).ToArray() });

    if (path.StartsWith("/preapproval/", StringComparison.Ordinal))
    {
      if (!Assinaturas.TryGetValue(path.Split('/').Last(), out var estado)) return new(HttpStatusCode.NotFound);

      if (request.Method == HttpMethod.Put) estado.Status = "cancelled";

      return Responder(Preapproval(estado));
    }

    if (path == "/authorized_payments/search")
    {
      var encontrados = Assinaturas.Values.Where(x => query.ContainsKey("payment_id")
        ? x.PagamentoId.ToString() == query["payment_id"] : x.Id == query["preapproval_id"]).Where(x => x.PagamentoId > 0).ToArray();

      return Responder(new
      {
        paging = new { total = encontrados.Length },
        results = encontrados.Select(x => Fatura(x)).ToArray()
      });
    }

    if (path.StartsWith("/authorized_payments/", StringComparison.Ordinal))
      return Responder(Fatura(Assinaturas.Values.Single(x => x.PagamentoId.ToString() == path.Split('/').Last())));

    if (path.StartsWith("/v1/payments/", StringComparison.Ordinal))
    {
      var estado = Assinaturas.Values.Single(x => x.PagamentoId.ToString() == path.Split('/').Last());

      return Responder(new
      {
        id = estado.PagamentoId,
        status = estado.StatusPagamento,
        external_reference = estado.Referencia,
        transaction_amount = estado.Valor,
        transaction_amount_refunded = estado.StatusPagamento == "refunded" ? estado.Valor : 0,
        currency_id = "BRL",
        collector_id = ColetorIncorreto ? 999 : 123,
        live_mode = false,
        date_approved = estado.Inicio,
        date_last_updated = estado.AtualizadaEm,
        date_created = estado.Inicio
      });
    }

    throw new InvalidOperationException("Endpoint inesperado: " + path);
  }

  private object Preapproval(Estado x) => new
  {
    id = x.Id,
    external_reference = x.Referencia,
    status = x.Status,
    init_point = "https://www.mercadopago.com.br/subscriptions/checkout?preapproval_id=" + x.Id,
    auto_recurring = new { frequency = 1, frequency_type = "months", transaction_amount = x.Valor, currency_id = "BRL" },
    collector_id = 123
  };

  private object Fatura(Estado x) => new
  {
    id = x.PagamentoId,
    preapproval_id = x.Id,
    external_reference = x.Referencia,
    transaction_amount = (ValorIncorreto ? x.Valor + 1 : x.Valor).ToString(System.Globalization.CultureInfo.InvariantCulture),
    currency_id = "BRL",
    debit_date = x.Inicio,
    payment = new { id = x.PagamentoId }
  };

  private static HttpResponseMessage Responder(object body) => new(HttpStatusCode.OK) { Content = JsonContent.Create(body) };

  internal sealed class Estado
  {
    public string Id { get; init; } = "";

    public string Referencia { get; init; } = "";

    public decimal Valor { get; init; }

    public string Status { get; set; } = "pending";

    public long PagamentoId { get; set; }

    public string StatusPagamento { get; set; } = "pending";

    public DateTimeOffset Inicio { get; set; } = DateTimeOffset.UtcNow.AddMinutes(-1);

    public DateTimeOffset AtualizadaEm { get; set; } = DateTimeOffset.UtcNow;
  }
}
