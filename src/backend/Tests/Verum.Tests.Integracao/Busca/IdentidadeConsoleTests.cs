using Verum.Modules.Busca.Aplicacao.Descoberta;
using Verum.Modules.Busca.Contratos.DTOs;
using Xunit;

namespace Verum.Tests.Integracao;

public sealed class IdentidadeConsoleTests
{
  [Theory]
  [InlineData("PlayStation 5 Slim", "Cooler LED RGB Para Playstation 5 Slim Pro 3 Ventoinhas 1 Usb 3.0", false)]
  [InlineData("PlayStation 5 Slim", "Fonte Alimentação 8,5v 5,6a Compatível Playstation 2 Slim 70000 Series", false)]
  [InlineData("PlayStation 5 Slim", "Tampa para Console PlayStation 5 Slim, Techno Red", false)]
  [InlineData("PlayStation 5 Slim", "Tampas Capas Ps5 Slim Chroma Teal Verde Playstation 5 Sony", false)]
  [InlineData("PlayStation 5 Slim", "Leitor Drive De Disco Console Ps5 Playstation 5 Slim Cor Branco", false)]
  [InlineData("PlayStation 5 Slim", "Unidade De Disco Para Ps5 Slim Sony Edição Digital", false)]
  [InlineData("PlayStation 5 Slim", "Console Playstation 5 Slim Digital 825gb SSD 1 Controle Dualsense", true)]
  [InlineData("cooler ps5 slim", "Cooler LED RGB Para Playstation 5 Slim Pro", true)]
  [InlineData("leitor ps5 slim", "Leitor De Disco Para Playstation 5 Slim", true)]
  [InlineData("fonte playstation 2 slim", "Fonte Alimentação 8,5v 5,6a Compatível Playstation 2 Slim", true)]
  [InlineData("iphone 15", "iPhone 5 15W", false)]
  [InlineData("PlayStation 5 Slim", "Base Stand Vertical Compativel Com Playstation 5 Ps5 Slim Fat Pro (BRANCO)", false)]
  [InlineData("PlayStation 5 Slim", "Stand Mesa Base Vertical Playstation 5 Ps5 Slim Fat Todos (Preto)", false)]
  [InlineData("PlayStation 5 Slim", "PlayStation 5 Slim Ultra HD Blu-ray Disc Drive, Black [video game]", false)]
  [InlineData("base vertical ps5 slim", "Base Vertical para PS5/PS5 Slim/PS5 Pro", true)]
  [InlineData("PlayStation 5 Slim", "Sony PlayStation 5 Slim Digital Console 825GB - International Version", true)]
  public void ModeloEIdentidadePrecedemPreco(string consulta, string titulo, bool elegivel) =>
    Assert.Equal(elegivel, NormalizacaoOferta.Relevancia(consulta, titulo) > 30);

  [Fact]
  public void SnapshotAntigoOuCacheNaoReintroduzPecasEOutroModelo()
  {
    OfertaResultado Item(string nome) => new(Guid.NewGuid(), Guid.NewGuid(), nome, "https://loja.example/p", 100,
      "PIX", null, "teste", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMinutes(10), 1, 90, "Antigo");

    var antiga = new BuscaConsulta(Guid.NewGuid(), "Concluida", "Resultado", false, null, null, 1, 1, 0,
      [Item("Fonte Alimentação Playstation 2 Slim 5,6a"), Item("Console PlayStation 5 Slim com Controle")]);

    var atual = BuscaService.Revalidar(antiga, "PlayStation 5 Slim");

    Assert.Single(atual.Ofertas);

    Assert.Equal(1, atual.Ofertas[0].Posicao);

    Assert.True(atual.Parcial);

    Assert.Equal(2, antiga.Ofertas.Count);
  }
}
