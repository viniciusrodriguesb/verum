using System.Net.Mail;
using System.Text.Json;

namespace Verum.Modules.Acesso.Dominio;

internal static class Validacao
{
  internal static string Texto(string valor, string nome, int maximo)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(valor, nome);

    valor = valor.Trim();

    if (valor.Length > maximo) throw new ArgumentException($"Máximo de {maximo} caracteres.", nome);

    return valor;
  }

  internal static Guid Identificador(Guid valor, string nome)
  {
    if (valor == Guid.Empty) throw new ArgumentException("Identificador vazio.", nome);

    return valor;
  }

  internal static T Enumeracao<T>(T valor, string nome) where T : struct, Enum
  {
    if (!Enum.IsDefined(valor)) throw new ArgumentException("Valor de enum inválido.", nome);

    return valor;
  }

  internal static DateTimeOffset Data(DateTimeOffset valor, string nome)
  {
    if (valor == default) throw new ArgumentException("Data deve ser informada.", nome);

    return valor.ToUniversalTime();
  }

  internal static decimal Numero(decimal valor, string nome, decimal minimo, decimal maximo, int escala)
  {
    if (valor < minimo || valor > maximo || decimal.Round(valor, escala) != valor)
      throw new ArgumentOutOfRangeException(nome, $"Valor deve estar entre {minimo} e {maximo}, com até {escala} casas decimais.");

    return valor;
  }

  internal static string Json(string valor, string nome)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(valor, nome);

    try
    {
      using var document = JsonDocument.Parse(valor);

      if (document.RootElement.ValueKind != JsonValueKind.Object)
        throw new ArgumentException("Esperado um objeto JSON.", nome);
    }
    catch (JsonException ex) {
      throw new ArgumentException("JSON inválido.", nome, ex);
    }

    return valor;
  }

  internal static string Url(string valor, string nome)
  {
    valor = Texto(valor, nome, 8192);

    if (!Uri.TryCreate(valor, UriKind.Absolute, out var uri) ||
        (uri.Scheme != "https" && uri.Scheme != "http") || !string.IsNullOrEmpty(uri.UserInfo))
      throw new ArgumentException("Esperada uma URL HTTP/HTTPS absoluta sem credenciais.", nome);

    return valor;
  }

  internal static string Email(string valor, string nome)
  {
    valor = Texto(valor, nome, 320);

    if (!MailAddress.TryCreate(valor, out var address) || address.Address != valor)
      throw new ArgumentException("E-mail inválido.", nome);

    return valor;
  }

  internal static string Hash(string valor, string nome)
  {
    valor = Texto(valor, nome, 64);

    if (valor.Length != 64 || valor.Any(c => !Uri.IsHexDigit(c)))
      throw new ArgumentException("Esperado SHA-256 hexadecimal de 64 caracteres.", nome);

    return valor.ToLowerInvariant();
  }

  internal static string Moeda(string valor, string nome)
  {
    valor = Texto(valor, nome, 3).ToUpperInvariant();

    if (valor.Length != 3 || valor.Any(c => c < 'A' || c > 'Z'))
      throw new ArgumentException("Esperado código de moeda de três letras.", nome);

    return valor;
  }

  internal static string Dominio(string valor, string nome)
  {
    valor = Texto(valor, nome, 255).ToLowerInvariant();

    if (Uri.CheckHostName(valor) != UriHostNameType.Dns)
      throw new ArgumentException("Domínio DNS inválido.", nome);

    return valor;
  }
}

