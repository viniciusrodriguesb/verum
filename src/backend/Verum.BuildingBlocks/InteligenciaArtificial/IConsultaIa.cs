namespace Verum.BuildingBlocks.InteligenciaArtificial;

public interface IConsultaIa
{
  Task<RespostaIa> ConsultarAsync(ConsultaIa consulta, CancellationToken cancellationToken = default);
}

public interface IProvedorIa
{
  string Nome { get; }

  Task<RespostaIa> ConsultarAsync(ConsultaIa consulta, CancellationToken cancellationToken = default);
}

