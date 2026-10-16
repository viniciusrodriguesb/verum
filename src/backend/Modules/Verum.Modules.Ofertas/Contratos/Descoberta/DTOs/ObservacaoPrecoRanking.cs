namespace Verum.Modules.Ofertas.Contratos.Descoberta.DTOs;

public sealed record ObservacaoPrecoRanking(Guid LojaId, DateTimeOffset ObservadaEm, decimal Preco, bool Pix);
