using Verum.BuildingBlocks.Erros;

namespace Verum.Modules.Assinaturas.Aplicacao.Gateways;

internal sealed class SelecionadorGateway(IEnumerable<IGatewayAssinaturas> gateways)
{
  public IGatewayAssinaturas Obter(string nome) =>
    gateways.SingleOrDefault(x => x.Nome == nome)
    ?? throw new ErroAplicacaoException("GATEWAY_NAO_CONFIGURADO", "Gateway de pagamento indisponível.", TipoErro.Indisponivel);
}
