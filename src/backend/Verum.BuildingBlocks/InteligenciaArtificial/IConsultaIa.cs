namespace Verum.BuildingBlocks.InteligenciaArtificial;

public interface IConsultaIa
{
  bool Disponivel => true;

  bool EstaDisponivel(string? provedor = null) => Disponivel;

  Task<RespostaIa> ConsultarAsync(ConsultaIa consulta, CancellationToken cancellationToken = default);
}

public interface IProvedorIa
{
  bool Configurado => true;

  string Nome { get; }

  Task<RespostaIa> ConsultarAsync(ConsultaIa consulta, CancellationToken cancellationToken = default);
}
