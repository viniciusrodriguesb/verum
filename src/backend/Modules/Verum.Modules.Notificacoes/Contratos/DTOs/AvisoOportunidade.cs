namespace Verum.Modules.Notificacoes.Contratos.DTOs;

public sealed record AvisoOportunidade(Guid Id, Guid ContaId, string Nome, decimal Preco,
  string Url, DateTimeOffset ExpiraEm, Guid OfertaId);
