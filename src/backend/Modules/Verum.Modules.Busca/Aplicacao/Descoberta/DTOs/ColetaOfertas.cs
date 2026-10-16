using Verum.Modules.Ofertas.Contratos.Descoberta.DTOs;

namespace Verum.Modules.Busca.Aplicacao.Descoberta.DTOs;

internal sealed record ColetaOfertas(IReadOnlyList<OfertaDescoberta> Ofertas, IReadOnlyList<DiagnosticoFonte> Fontes);
