using Verum.Modules.Busca.Contratos.InteligenciaArtificial.DTOs;

namespace Verum.Modules.Busca.Contratos.InteligenciaArtificial;

public interface IDescobertaProdutosIaService
{
  bool Disponivel => true;

  Task<DescobertaProdutosIa> ConsultarAsync(string consulta, CancellationToken cancellationToken = default);
}
