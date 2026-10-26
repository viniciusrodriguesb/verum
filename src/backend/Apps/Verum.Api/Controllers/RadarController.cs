using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verum.Modules.Assinaturas.Contratos;
using Verum.Modules.Contas.Contratos;
using Verum.Modules.Ofertas.Contratos.Radar;
using Verum.Modules.Radar.Contratos;
using Verum.Modules.Radar.Contratos.DTOs;

namespace Verum.Api.Controllers;

public sealed class RadarController(IRadarService radar, IContaService contas, IOfertaRadarService ofertas,
  IAssinaturasService assinaturas) : AppController
{
  [AllowAnonymous]
  [HttpGet("ofertas/{id:guid}")]
  public async Task<IActionResult> Oferta(Guid id, CancellationToken cancellationToken)
  {
    Response.Headers.CacheControl = "no-store";

    var oferta = await ofertas.ObterAsync(id, cancellationToken);

    return oferta is null ? NotFound() : Ok(oferta);
  }

  [AllowAnonymous]
  [HttpGet("capacidade")]
  public async Task<ActionResult<CapacidadeRadar>> Capacidade(CancellationToken cancellationToken)
  {
    Response.Headers.CacheControl = "no-store";

    var conta = User.Identity?.IsAuthenticated == true ? await SincronizarContaAsync(contas, cancellationToken) : null;

    var capacidade = await radar.CapacidadeAsync(conta?.Id, cancellationToken);

    var planos = await assinaturas.ListarPlanosAsync(cancellationToken);

    return Ok(capacidade with { VendaDisponivel = planos.Any(x => x.Codigo == capacidade.PlanoNecessario) });
  }

  [HttpGet]
  public async Task<ActionResult<IReadOnlyList<MonitoramentoConsulta>>> Listar(CancellationToken cancellationToken)
  {
    Response.Headers.CacheControl = "no-store";

    var conta = await SincronizarContaAsync(contas, cancellationToken);

    return Ok(await radar.ListarAsync(conta.Id, cancellationToken));
  }

  [HttpPost]
  public async Task<ActionResult<MonitoramentoConsulta>> Criar(CriarMonitoramento pedido, CancellationToken cancellationToken)
  {
    var conta = await SincronizarContaAsync(contas, cancellationToken);

    return Ok(await radar.CriarAsync(conta.Id, pedido, cancellationToken));
  }

  [HttpPatch("{id:guid}")]
  public async Task<IActionResult> Alterar(Guid id, AlterarMonitoramento pedido, CancellationToken cancellationToken)
  {
    var conta = await SincronizarContaAsync(contas, cancellationToken);

    await radar.AlterarAsync(conta.Id, id, pedido.PrecoAlvo, pedido.Acao, cancellationToken);

    return NoContent();
  }
}
