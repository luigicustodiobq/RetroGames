using Microsoft.AspNetCore.Mvc;
using RetroGames.Services;
using RetroGames.WebApi.ApiModels;
using RetroGames.WebApi.Mappers;

namespace RetroGames.WebApi.Controllers;

/// <summary>Rota de consulta das condições de jogo (pública, somente leitura).</summary>
[ApiController]
[Route("api/[controller]")]
public class CondicaoJogoController : ControllerBase
{
    private readonly ICondicaoJogoService condicaoJogoService;

    public CondicaoJogoController(ICondicaoJogoService condicaoJogoService)
    {
        this.condicaoJogoService = condicaoJogoService;
    }

    [HttpGet]
    public async Task<ActionResult<RetroGamesResponseApiModel<List<ListarCondicaoJogoResponseApiModel>>>> Listar()
    {
        var condicoes = await condicaoJogoService.ListarAsync();
        var resposta = condicoes.Select(condicao => condicao.ParaResponse()).ToList();
        return Ok(new RetroGamesResponseApiModel<List<ListarCondicaoJogoResponseApiModel>>(resposta));
    }
}
