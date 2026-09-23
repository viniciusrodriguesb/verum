using System.Security.Cryptography;
using System.Text;

namespace Verum.CrossCutting.Cache;

public static class ChavesCache
{
  public static string Id(Guid id)
  {
    if (id == Guid.Empty) throw new ArgumentException("O identificador não pode ser vazio.", nameof(id));

    return id.ToString("N");
  }

  public static string Hash(string valor)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(valor);

    return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(valor)));
  }

  internal static string Validar(string chave)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(chave);

    if (chave.Length > 512) throw new ArgumentException("A chave deve ter até 512 caracteres.", nameof(chave));

    return chave;
  }
}
