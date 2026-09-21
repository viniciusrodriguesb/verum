namespace Verum.Modules.Notificacoes.Dominio;

internal sealed partial class Notificacao
{
  private readonly List<EntregaNotificacao> _entregas = [];
  public IReadOnlyCollection<EntregaNotificacao> Entregas => _entregas.AsReadOnly();
}

