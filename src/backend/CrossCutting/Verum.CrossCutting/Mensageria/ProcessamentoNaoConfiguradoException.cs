namespace Verum.CrossCutting.Mensageria;

// Falha permanente do esqueleto: não confirmar trabalho inexistente nem repeti-lo.
public sealed class ProcessamentoNaoConfiguradoException(string consumidor)
  : InvalidOperationException($"O processamento de {consumidor} ainda não foi implementado.");

