using System.Globalization;
using System.Text.RegularExpressions;

namespace Verum.Modules.Ofertas.Contratos.Descoberta;

public static class NormalizacaoPreco
{
  public static bool Tentar(string? texto, out decimal preco)
  {
    preco = 0;

    if (texto is null) return false;

    texto = Regex.Replace(texto.Trim(), @"^R\$\s*", "");

    var brasileiro = Regex.IsMatch(texto, @"^(?:\d{1,3}(?:\.\d{3})*|\d+),\d{2}$");

    var internacional = Regex.IsMatch(texto, @"^(?:\d{1,3}(?:,\d{3})*|\d+)\.\d{2}$");

    return (brasileiro || internacional) && decimal.TryParse(texto, NumberStyles.Number,
      CultureInfo.GetCultureInfo(brasileiro ? "pt-BR" : "en-US"), out preco);
  }
}

