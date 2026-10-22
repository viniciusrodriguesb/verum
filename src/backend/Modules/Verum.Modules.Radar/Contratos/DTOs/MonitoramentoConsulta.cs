namespace Verum.Modules.Radar.Contratos.DTOs;

public sealed record MonitoramentoConsulta(Guid Id, string Nome, Guid OfertaId, Guid VarianteId,
  decimal PrecoInicial, decimal PrecoAlvo, decimal? PrecoObservado, string Estado,
  DateTimeOffset CriadoEm, DateTimeOffset? VerificadaEm, DateTimeOffset? ProximaVerificacaoEm,
  string? UrlOferta, string? Fonte, string Condicao, string? ImagemUrl);
