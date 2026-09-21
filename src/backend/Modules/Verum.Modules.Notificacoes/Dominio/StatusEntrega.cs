namespace Verum.Modules.Notificacoes.Dominio;

internal enum StatusEntrega : short
{
  Pendente = 1,
  Processando = 2,
  Entregue = 3,
  AguardandoNovaTentativa = 4,
  FalhouDefinitivamente = 5,
  Cancelada = 6,
}

