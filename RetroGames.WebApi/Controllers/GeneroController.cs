using Microsoft.AspNetCore.Mvc;
using RetroGames.Services;
using RetroGames.WebApi.ApiModels;
using RetroGames.WebApi.Mappers;

namespace RetroGames.WebApi.Controllers;

/// <summary>Rota de consulta dos gêneros (pública, somente leitura).</summary>
[ApiController]
[Route("api/[controller]")]
public class GeneroController : ControllerBase
{
    private readonly IGeneroService generoService;

    public GeneroController(IGeneroService generoService)
    {
        this.generoService = generoService;
    }

    [HttpGet]
    public async Task<ActionResult<RetroGamesResponseApiModel<List<ListarGeneroResponseApiModel>>>> Listar()
    {
        var generos = await generoService.ListarAsync();
        var resposta = generos.Select(genero => genero.ParaResponse()).ToList();
        return Ok(new RetroGamesResponseApiModel<List<ListarGeneroResponseApiModel>>(resposta));
    }
}
