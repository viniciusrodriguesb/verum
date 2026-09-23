namespace Verum.Modules.Busca.Contratos.InteligenciaArtificial;

public interface IDescobertaProdutosIaService
{
  Task<DescobertaProdutosIa> ConsultarAsync(string consulta, CancellationToken cancellationToken = default);
}

