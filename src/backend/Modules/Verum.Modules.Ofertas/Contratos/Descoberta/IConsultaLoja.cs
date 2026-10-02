using Verum.Modules.Ofertas.Contratos.Descoberta.DTOs;

namespace Verum.Modules.Ofertas.Contratos.Descoberta;

public interface IConsultaLoja
{
  bool Disponivel => true;

  Task<ResultadoConsultaLoja> ConsultarAsync(string loja, string consulta, CancellationToken cancellationToken = default);
}
