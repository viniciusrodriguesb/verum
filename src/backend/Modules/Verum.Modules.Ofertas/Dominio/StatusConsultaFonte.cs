namespace Verum.Modules.Ofertas.Dominio;

internal enum StatusConsultaFonte : short
{
  Pendente = 1,
  Executando = 2,
  Concluida = 3,
  ConcluidaSemResultado = 4,
  Falhou = 5,
  IgnoradaPorCircuitBreaker = 6,
  Expirada = 7,
}

