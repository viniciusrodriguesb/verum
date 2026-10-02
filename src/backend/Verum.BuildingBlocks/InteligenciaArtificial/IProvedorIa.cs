using Verum.BuildingBlocks.InteligenciaArtificial.DTOs;

namespace Verum.BuildingBlocks.InteligenciaArtificial;

public interface IProvedorIa
{
  bool Configurado => true;

  string Nome { get; }

  Task<RespostaIa> ConsultarAsync(ConsultaIa consulta, CancellationToken cancellationToken = default);
}
