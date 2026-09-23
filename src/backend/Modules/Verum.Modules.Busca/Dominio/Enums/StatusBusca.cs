namespace Verum.Modules.Busca.Dominio;

internal enum StatusBusca : short
{
  Recebida = 1,
  Processando = 2,
  Concluida = 3,
  ConcluidaSemResultado = 4,
  Falhou = 5,
  Expirada = 6,
}

