namespace Verum.BuildingBlocks.Erros;

// Apenas mensagens seguras para o consumidor da aplicação. Detalhes técnicos ficam na InnerException.
public sealed class ErroAplicacaoException : Exception
{
  public string Codigo { get; }

  public TipoErro Tipo { get; }

  public string? Campo { get; }

  public ErroAplicacaoException(string codigo, string mensagem, TipoErro tipo, string? campo = null, Exception? innerException = null)
    : base(mensagem, innerException)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(codigo);

    ArgumentException.ThrowIfNullOrWhiteSpace(mensagem);

    if (!Enum.IsDefined(tipo)) throw new ArgumentOutOfRangeException(nameof(tipo));

    Codigo = codigo;

    Tipo = tipo;

    Campo = campo;
  }

  public static ErroAplicacaoException Validacao(string mensagem, string? campo = null, Exception? innerException = null) =>
    new("VALIDACAO", mensagem, TipoErro.Validacao, campo, innerException);
}
