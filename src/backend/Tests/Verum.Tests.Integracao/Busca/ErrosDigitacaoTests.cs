using Verum.Modules.Busca.Aplicacao.Descoberta;
using Xunit;

namespace Verum.Tests.Integracao;

public sealed class ErrosDigitacaoTests
{
  [Theory]
  [InlineData("grampeadro spiral", "Grampeador Spiral 26/6", true)]
  [InlineData("torneirra cozinha", "Torneira para Cozinha Inox", true)]
  [InlineData("bola volei mikassa", "Bola de Vôlei Mikasa", true)]
  [InlineData("caderno universitario", "Caderno Universitário 10 Matérias", true)]
  [InlineData("O homem mais rico da Babilônia", "Homem Mais Rico da Babilonia, O", true)]
  [InlineData("samnsung s25", "Samsung Galaxy S25", true)]
  [InlineData("samnsung s25", "Samsung Galaxy S25 Ultra", false)]
  [InlineData("samnsung s25", "Samsung Galaxy S24", false)]
  [InlineData("iphone 17 256gb", "iPhone 17 128GB", false)]
  [InlineData("cabo scsi hd68", "Cabo SCSI HD50", false)]
  [InlineData("grampeadro spirla", "Grampeador Spiral", false)]
  [InlineData("o da de", "Produto aleatório", false)]
  [InlineData("Playstation 5 Slim", "Fonte Playstation 2 Slim 5,6a", false)]
  [InlineData("grampeador spiral", "Grampeador Spiral com Base Antiderrapante", true)]
  [InlineData("grampeador spiral", "Grampo para Grampeador Spiral 26/6", false)]
  [InlineData("grampo grampeador spiral", "Grampo para Grampeador Spiral 26/6", true)]
  [InlineData("barraca camping", "Lona para Barraca Camping", false)]
  [InlineData("barraca camping", "Barraca Camping com Bolsa e Suportes", true)]
  public void ToleraErroPequenoSemRelaxarIdentidadeNumerica(string consulta, string titulo, bool aceita) =>
    Assert.Equal(aceita, NormalizacaoOferta.Relevancia(consulta, titulo) > 30m);
}
