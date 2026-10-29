using Verum.Modules.Busca.Aplicacao.Descoberta;
using Xunit;

namespace Verum.Tests.Integracao;

public sealed class IdentidadeProdutosVariadosTests
{
  [Theory]
  [InlineData("air fryer mondial", "2 Botões Vermelho Air Fryer Mondial AFN-40 Timer Temperatura", false)]
  [InlineData("aspirador electrolux", "Kit 3 Sacos de aspirador de pó Electrolux CSE10", false)]
  [InlineData("aspirador electrolux", "Rodinha Frontal Original Electrolux Aspirador Erg23", false)]
  [InlineData("aspirador electrolux", "Reservatório Recipiente Coletor de Pó para Aspirador Electrolux STK14", false)]
  [InlineData("aspirador electrolux", "Kit Performance para Robô Aspirador Electrolux", false)]
  [InlineData("panela pressão tramontina", "Anel de Vedação para Panela de Pressão Tramontina", false)]
  [InlineData("aspirador electrolux", "Bocal Bico Acessório Escova Electrolux Aspirador", false)]
  [InlineData("panela pressão tramontina", "Borracha Para Panela de Pressão Tramontina Vancouver", false)]
  [InlineData("playstation 5", "Playstation 5 DualSense Wireless Controller Genshin Impact", false)]
  [InlineData("furadeira bosch", "Bosch Mandril Para Furadeira Com Chave 13Mm", false)]
  [InlineData("panela pressão tramontina", "Peso Pino Panela Pressão Universal Compatível Tramontina", false)]
  [InlineData("óculos natação nabaiji", "Caixa para Óculos de Natação Nabaiji Preta", false)]
  [InlineData("raquete tênis artengo", "Raqueteira para Raquete de Tênis Artengo Preto", false)]
  [InlineData("playstation 5", "Street Fighter 6 Fighters Edition - PlayStation 5", false)]
  [InlineData("playstation 5", "Headset Gamer RGB PC PS4 PS5 Xbox Notebook", false)]
  [InlineData("playstation 5", "Console Playstation 5 Slim com Jogo e Controle Dualsense", true)]
  [InlineData("playstation 5", "Sony PlayStation 5 Slim Digital 825GB", true)]
  [InlineData("air fryer mondial", "Air Fryer Mondial 4L com Botão de Temperatura", true)]
  [InlineData("aspirador electrolux", "Aspirador Electrolux com Saco Descartável", true)]
  [InlineData("furadeira bosch", "Furadeira Bosch com Mandril 13mm", true)]
  [InlineData("panela pressão tramontina", "Panela de Pressão Tramontina com Válvula de Segurança", true)]
  [InlineData("óculos natação nabaiji", "Óculos de Natação Nabaiji com Caixa", true)]
  [InlineData("raquete tênis artengo", "Raquete de Tênis Artengo com Capa", true)]
  [InlineData("saco aspirador electrolux", "Saco para Aspirador Electrolux", true)]
  [InlineData("reservatório aspirador electrolux", "Reservatório para Aspirador Electrolux", true)]
  [InlineData("anel vedação panela pressão tramontina", "Anel de Vedação para Panela de Pressão Tramontina", true)]
  [InlineData("mandril furadeira bosch", "Bosch Mandril para Furadeira", true)]
  [InlineData("botão air fryer mondial", "Botão para Air Fryer Mondial", true)]
  public void PecasNaoSubstituemProdutoPrincipalMasContinuamPesquisaveis(string consulta, string titulo, bool elegivel) =>
    Assert.Equal(elegivel, NormalizacaoOferta.Relevancia(consulta, titulo) > 30);
}
