using Microsoft.AspNetCore.Mvc;
using RetroGames.Services;
using RetroGames.WebApi.ApiModels;
using RetroGames.WebApi.Mappers;

namespace RetroGames.WebApi.Controllers;

/// <summary>Rotas do catálogo de jogos.</summary>
[ApiController]
[Route("api/[controller]")]
public class JogoController : ControllerBase
{
    private readonly IJogoService jogoService;

    public JogoController(IJogoService jogoService)
    {
        this.jogoService = jogoService;
    }

    /// <summary>Catálogo paginado, com filtros opcionais por nome, plataforma e gênero.</summary>
    [HttpGet]
    public async Task<ActionResult<RetroGamesResponseApiModel<PaginaResponseApiModel<ListarJogoResponseApiModel>>>> Listar(
        [FromQuery] ListarJogoRequestApiModel request)
    {
        var pagina = await jogoService.ListarAsync(request.ParaFiltro());
        return Ok(new RetroGamesResponseApiModel<PaginaResponseApiModel<ListarJogoResponseApiModel>>(pagina.ParaResponse()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RetroGamesResponseApiModel<ListarPorIdJogoResponseApiModel>>> ListarPorId(int id)
    {
        var detalhe = await jogoService.BuscarPorIdAsync(id);
        return Ok(new RetroGamesResponseApiModel<ListarPorIdJogoResponseApiModel>(detalhe.ParaResponsePorId()));
    }
}
