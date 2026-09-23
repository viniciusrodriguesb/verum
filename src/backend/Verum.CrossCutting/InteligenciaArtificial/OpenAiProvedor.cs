using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Verum.BuildingBlocks.InteligenciaArtificial;

namespace Verum.CrossCutting.InteligenciaArtificial;

public sealed class OpenAiProvedor(IHttpClientFactory clients, IOptions<OpenAiOptions> options) : IProvedorIa
{
  public const string ClienteHttp = "IA.OpenAI";

  public string Nome => "OpenAI";

  public async Task<RespostaIa> ConsultarAsync(ConsultaIa consulta, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(consulta);

    ArgumentException.ThrowIfNullOrWhiteSpace(consulta.Contexto);

    ArgumentException.ThrowIfNullOrWhiteSpace(consulta.Entrada);

    ArgumentException.ThrowIfNullOrWhiteSpace(consulta.NomeFormato);

    if (consulta.Esquema.ValueKind != JsonValueKind.Object)
      throw new ArgumentException("O esquema de saída deve ser um objeto JSON.", nameof(consulta));

    var config = options.Value;

    if (string.IsNullOrWhiteSpace(config.ApiKey) || string.IsNullOrWhiteSpace(config.Modelo))
      throw new InvalidOperationException("Configure a chave e o modelo em InteligenciaArtificial:OpenAI.");

    var corpo = new Dictionary<string, object?>
    {
      ["model"] = config.Modelo,
      ["store"] = false,
      ["instructions"] = consulta.Contexto,
      ["input"] = consulta.Entrada,
      ["max_output_tokens"] = config.MaxOutputTokens,
      ["text"] = new
      {
        format = new { type = "json_schema", name = consulta.NomeFormato, strict = true, schema = consulta.Esquema }
      }
    };

    if (consulta.PesquisarWeb)
    {
      corpo["tools"] = new[] { new { type = "web_search" } };

      corpo["tool_choice"] = "required";

      corpo["max_tool_calls"] = config.MaxToolCalls;

      corpo["include"] = new[] { "web_search_call.action.sources" };
    }

    using var client = clients.CreateClient(ClienteHttp);

    using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses");

    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiKey);

    request.Content = JsonContent.Create(corpo);

    // POST não é repetido pelo pipeline HTTP; uma repetição pode gerar nova cobrança.
    using var response = await client.SendAsync(request, cancellationToken);

    response.EnsureSuccessStatusCode();

    try
    {
      using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);

      return LerResposta(document.RootElement, consulta.PesquisarWeb);
    }
    catch (JsonException)
    {
      throw new RespostaIaInvalidaException("JSON_INVALIDO");
    }
    catch (InvalidOperationException)
    {
      throw new RespostaIaInvalidaException("FORMATO_INVALIDO");
    }
    catch (KeyNotFoundException)
    {
      throw new RespostaIaInvalidaException("CAMPO_AUSENTE");
    }
  }

  private RespostaIa LerResposta(JsonElement root, bool exigirWeb)
  {
    if (root.GetProperty("status").GetString() != "completed")
      throw new RespostaIaInvalidaException("RESPOSTA_INCOMPLETA");

    var textos = new List<string>();

    var fontes = new List<FonteIa>();

    var pesquisou = false;

    foreach (var item in root.GetProperty("output").EnumerateArray())
    {
      var tipo = item.GetProperty("type").GetString();

      if (tipo == "web_search_call" && item.GetProperty("status").GetString() == "completed")
      {
        pesquisou = true;

        if (item.TryGetProperty("action", out var action) && action.TryGetProperty("sources", out var sources))
          foreach (var source in sources.EnumerateArray())
            AdicionarFonte(source, fontes);
      }

      if (tipo != "message") continue;

      foreach (var content in item.GetProperty("content").EnumerateArray())
      {
        var contentType = content.GetProperty("type").GetString();

        if (contentType == "refusal")
          throw new RespostaIaInvalidaException("RECUSA");

        if (contentType != "output_text") continue;

        textos.Add(content.GetProperty("text").GetString() ?? string.Empty);

        if (content.TryGetProperty("annotations", out var annotations))
          foreach (var annotation in annotations.EnumerateArray())
            if (annotation.GetProperty("type").GetString() == "url_citation")
              AdicionarFonte(annotation, fontes);
      }
    }

    if (exigirWeb && !pesquisou)
      throw new RespostaIaInvalidaException("PESQUISA_NAO_EXECUTADA");

    using var json = JsonDocument.Parse(string.Concat(textos));

    if (json.RootElement.ValueKind != JsonValueKind.Object)
      throw new RespostaIaInvalidaException("OBJETO_ESPERADO");

    var temUso = root.TryGetProperty("usage", out var usage) && usage.ValueKind == JsonValueKind.Object;

    return new RespostaIa(json.RootElement.Clone(), Nome,
      root.GetProperty("model").GetString() ?? options.Value.Modelo,
      root.GetProperty("id").GetString() ?? string.Empty, DateTimeOffset.UtcNow,
      temUso && usage.TryGetProperty("input_tokens", out var entrada) ? entrada.GetInt64() : null,
      temUso && usage.TryGetProperty("output_tokens", out var saida) ? saida.GetInt64() : null,
      fontes.DistinctBy(x => x.Url).ToArray());
  }

  private static void AdicionarFonte(JsonElement item, List<FonteIa> fontes)
  {
    if (!item.TryGetProperty("url", out var url)) return;

    if (!Uri.TryCreate(url.GetString(), UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https")) return;

    fontes.Add(new FonteIa(uri.AbsoluteUri, item.TryGetProperty("title", out var title) ? title.GetString() : null));
  }
}

