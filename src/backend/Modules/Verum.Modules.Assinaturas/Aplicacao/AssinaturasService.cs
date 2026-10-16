using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Verum.BuildingBlocks.Erros;
using Verum.BuildingBlocks.Identidade;
using Verum.Modules.Acesso.Contratos;
using Verum.Modules.Assinaturas.Aplicacao.Configuracoes;
using Verum.Modules.Assinaturas.Aplicacao.Gateways;
using Verum.Modules.Assinaturas.Contratos;
using Verum.Modules.Assinaturas.Contratos.DTOs;
using Verum.Modules.Assinaturas.Dominio;
using Verum.Modules.Assinaturas.Infraestrutura.Persistencia;

namespace Verum.Modules.Assinaturas.Aplicacao;

internal sealed class AssinaturasService(AssinaturasDbContext db, IOptions<AssinaturasOptions> options,
  SelecionadorGateway gateways, IAcessoBuscaService acesso) : IAssinaturasService
{
  public Task<IReadOnlyList<PlanoConsulta>> ListarPlanosAsync(CancellationToken cancellationToken) =>
    Task.FromResult<IReadOnlyList<PlanoConsulta>>(options.Value.Habilitado
      ? options.Value.PrecosMensais.Where(x => x.Value > 0).Select(x => new PlanoConsulta(x.Key, x.Value, "BRL")).ToArray() : []);

  public async Task<AssinaturaConsulta> CriarAsync(Guid contaId, string email, SolicitarAssinatura pedido, CancellationToken cancellationToken)
  {
    Validacao.Identificador(contaId, nameof(contaId));

    Validacao.Email(email, nameof(email));

    pedido = pedido with
    {
      ChaveIdempotencia = Validacao.Texto(pedido.ChaveIdempotencia, nameof(pedido.ChaveIdempotencia), 100),

      Plano = Validacao.Texto(pedido.Plano, nameof(pedido.Plano), 80)
    };

    if (!options.Value.Habilitado)
      throw new ErroAplicacaoException("PAGAMENTOS_DESABILITADOS", "As contratações estão temporariamente indisponíveis.", TipoErro.Indisponivel);

    await acesso.PrepararAsync(AtorBusca.Conta(contaId), cancellationToken);

    await using var transacao = await db.Database.BeginTransactionAsync(cancellationToken);

    await db.Database.ExecuteSqlInterpolatedAsync(
      $"SELECT pg_advisory_xact_lock(hashtextextended({contaId.ToString()}, 710010))", cancellationToken);

    var existente = await db.Assinaturas.Include(x => x.Plano)
      .SingleOrDefaultAsync(x => x.ContaId == contaId && x.ChaveIdempotencia == pedido.ChaveIdempotencia, cancellationToken);

    if (existente is not null)
    {
      if (existente.Plano.Codigo != pedido.Plano)
        throw new ErroAplicacaoException("IDEMPOTENCIA_CONFLITO", "A chave já foi usada para outro plano.", TipoErro.Conflito);

      await RecuperarAsync(existente, cancellationToken);

      await db.SaveChangesAsync(cancellationToken);

      await transacao.CommitAsync(cancellationToken);

      return Converter(existente);
    }

    if (!options.Value.PrecosMensais.TryGetValue(pedido.Plano, out var preco) || preco <= 0)
      throw new ErroAplicacaoException("PLANO_INDISPONIVEL", "Plano indisponível para contratação.", TipoErro.Validacao);

    if (await db.Assinaturas.AnyAsync(x => x.ContaId == contaId && x.Status != StatusAssinatura.Cancelada
      && x.Status != StatusAssinatura.Expirada, cancellationToken))
      throw new ErroAplicacaoException("ASSINATURA_EXISTENTE", "Você já possui uma assinatura ou contratação em andamento.", TipoErro.Conflito);

    var gateway = gateways.Obter(options.Value.Gateway);

    await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(710011)", cancellationToken);

    var tipo = Enum.Parse<GatewayPagamento>(gateway.Nome);

    var plano = await db.Planos.SingleOrDefaultAsync(x => x.Codigo == pedido.Plano, cancellationToken);

    if (plano is null)
    {
      plano = new Plano(pedido.Plano, pedido.Plano, "Assinatura mensal Verum", pedido.Plano, preco, "BRL", PeriodicidadePlano.Mensal);

      db.Planos.Add(plano);
    }
    else
    {
      if (!plano.Ativo)
        throw new ErroAplicacaoException("PLANO_INDISPONIVEL", "Plano indisponível para contratação.", TipoErro.Validacao);

      plano.AtualizarPreco(preco);
    }

    var cliente = await db.ClientesGateway.SingleOrDefaultAsync(x => x.ContaId == contaId && x.Gateway == tipo, cancellationToken);

    if (cliente is null)
    {
      // Correlação local; os dados de pagamento são coletados somente no checkout hospedado.
      cliente = new ClienteGateway(contaId, tipo, "conta:" + contaId);

      db.ClientesGateway.Add(cliente);
    }

    var assinatura = new Assinatura(contaId, plano.Id, cliente.Id, tipo, "pendente:" + Guid.NewGuid(), preco, "BRL");

    assinatura.DefinirPedido(pedido.ChaveIdempotencia);

    db.Assinaturas.Add(assinatura);

    await db.SaveChangesAsync(cancellationToken);

    // Persiste antes do POST: uma queda será recuperada pela referência, sem repetir a criação.
    await transacao.CommitAsync(cancellationToken);

    var remota = await gateway.CriarAsync(assinatura.Id, email, pedido.Plano, preco, cancellationToken);

    ValidarRemota(assinatura, remota.Referencia, remota.Valor, remota.Moeda);

    assinatura.VincularGateway(remota.Id, remota.UrlCheckout);

    await db.SaveChangesAsync(cancellationToken);

    return Converter(assinatura, plano.Codigo);
  }

  public async Task<AssinaturaConsulta> ObterAsync(Guid contaId, Guid id, CancellationToken cancellationToken) =>
    Converter(await LocalAsync(contaId, id, cancellationToken));

  public async Task<IReadOnlyList<AssinaturaConsulta>> ListarAsync(Guid contaId, CancellationToken cancellationToken)
  {
    var assinaturas = await db.Assinaturas.AsNoTracking().Include(x => x.Plano).Where(x => x.ContaId == contaId)
      .OrderByDescending(x => x.CriadaEm).Take(20).ToListAsync(cancellationToken);

    return assinaturas.Select(x => Converter(x)).ToArray();
  }

  public async Task CancelarAsync(Guid contaId, Guid id, CancellationToken cancellationToken)
  {
    await using var transacao = await db.Database.BeginTransactionAsync(cancellationToken);

    await db.Database.ExecuteSqlInterpolatedAsync(
      $"SELECT pg_advisory_xact_lock(hashtextextended({id.ToString()}, 710012))", cancellationToken);

    var assinatura = await LocalAsync(contaId, id, cancellationToken);

    assinatura.SolicitarCancelamento();

    var chave = "cancelamento:" + assinatura.Id;

    if (!await db.EventosGateway.AnyAsync(x => x.Gateway == assinatura.Gateway && x.IdentificadorExterno == chave, cancellationToken))
      db.EventosGateway.Add(new EventoGateway(assinatura.Gateway, chave, "reconciliacao",
        JsonSerializer.Serialize(new { recursoId = assinatura.Id.ToString() })));

    // O processador confirma o cancelamento mesmo após indisponibilidade do gateway.
    await db.SaveChangesAsync(cancellationToken);

    await transacao.CommitAsync(cancellationToken);
  }

  private async Task RecuperarAsync(Assinatura assinatura, CancellationToken cancellationToken)
  {
    if (!assinatura.IdentificadorExterno.StartsWith("pendente:", StringComparison.Ordinal)) return;

    var remota = await gateways.Obter(assinatura.Gateway.ToString()).LocalizarAsync(assinatura.Id, cancellationToken);

    if (remota is null)
      throw new ErroAplicacaoException("ASSINATURA_EM_CONFIRMACAO",
        "A contratação está sendo confirmada. Consulte novamente; não inicie outro pagamento.", TipoErro.Conflito);

    ValidarRemota(assinatura, remota.Referencia, remota.Valor, remota.Moeda);

    assinatura.VincularGateway(remota.Id, remota.UrlCheckout);
  }

  private async Task<Assinatura> LocalAsync(Guid contaId, Guid id, CancellationToken cancellationToken) =>
    await db.Assinaturas.Include(x => x.Plano).SingleOrDefaultAsync(x => x.Id == id && x.ContaId == contaId, cancellationToken)
    ?? throw new ErroAplicacaoException("ASSINATURA_NAO_ENCONTRADA", "Assinatura não encontrada.", TipoErro.NaoEncontrado);

  private static void ValidarRemota(Assinatura assinatura, Guid referencia, decimal valor, string moeda)
  {
    if (referencia != assinatura.Id || valor != assinatura.ValorContratado || moeda != assinatura.Moeda)
      throw new ErroAplicacaoException("GATEWAY_RESPOSTA_INVALIDA", "Dados de contratação incompatíveis.", TipoErro.Conflito);
  }

  private static AssinaturaConsulta Converter(Assinatura assinatura, string? plano = null) =>
    new(assinatura.Id, plano ?? assinatura.Plano.Codigo, assinatura.Status.ToString(), assinatura.ValorContratado, assinatura.Moeda,
      assinatura.CancelamentoSolicitadoEm.HasValue || assinatura.Status != StatusAssinatura.Pendente ? null : assinatura.UrlCheckout, assinatura.PeriodoTerminaEm,
      assinatura.CancelamentoSolicitadoEm.HasValue);
}
