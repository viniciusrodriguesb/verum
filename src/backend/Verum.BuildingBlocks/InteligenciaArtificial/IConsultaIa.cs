using Verum.BuildingBlocks.InteligenciaArtificial.DTOs;

namespace Verum.BuildingBlocks.InteligenciaArtificial;

public interface IConsultaIa
{
  bool Disponivel => true;

  bool EstaDisponivel(string? provedor = null) => Disponivel;

  Task<RespostaIa> ConsultarAsync(ConsultaIa consulta, CancellationToken cancellationToken = default);
}
