using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetroGames.Services;
using RetroGames.WebApi.ApiModels;
using RetroGames.WebApi.Mappers;

namespace RetroGames.WebApi.Controllers;

/// <summary>Rotas do Módulo de Compras. Todas exigem login (401 / responseCode 4 sem sessão).</summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PedidoController : ControllerBase
{
    private readonly IPedidoService pedidoService;

    public PedidoController(IPedidoService pedidoService)
    {
        this.pedidoService = pedidoService;
    }

    /// <summary>
    /// C04: finaliza a compra do carrinho. HTTP 201 com o pedido gravado e o header Location.
    /// Erros: 400 (código 3 ou 110 - jogo indisponível) e 409 (111 - estoque insuficiente).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RetroGamesResponseApiModel<PedidoResponseApiModel>>> Finalizar(
        [FromBody] FinalizarPedidoRequestApiModel request)
    {
        var pedido = await pedidoService.FinalizarCompraAsync(request.ParaItensCompra());
        return CreatedAtAction(
            nameof(BuscarPorId),
            new { id = pedido.Pedido.Id },
            new RetroGamesResponseApiModel<PedidoResponseApiModel>(pedido.ParaResponse()));
    }

    /// <summary>C05: pedidos do usuário logado, do mais recente para o mais antigo.</summary>
    [HttpGet("meus")]
    public async Task<ActionResult<RetroGamesResponseApiModel<List<PedidoResponseApiModel>>>> ListarMeus()
    {
        var pedidos = await pedidoService.ListarMeusPedidosAsync();
        var resposta = pedidos.Select(pedido => pedido.ParaResponse()).ToList();
        return Ok(new RetroGamesResponseApiModel<List<PedidoResponseApiModel>>(resposta));
    }

    /// <summary>Um pedido do usuário logado. Erros: 404 (109) e 403 (113 - pedido de outro usuário).</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RetroGamesResponseApiModel<PedidoResponseApiModel>>> BuscarPorId(int id)
    {
        var pedido = await pedidoService.BuscarMeuPedidoAsync(id);
        return Ok(new RetroGamesResponseApiModel<PedidoResponseApiModel>(pedido.ParaResponse()));
    }
}
