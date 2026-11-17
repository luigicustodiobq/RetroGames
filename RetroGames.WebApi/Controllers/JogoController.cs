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

    /// <summary>Grava o jogo com seus gêneros. HTTP 201 com o header Location.</summary>
    [HttpPost]
    public async Task<ActionResult<RetroGamesResponseApiModel<ListarPorIdJogoResponseApiModel>>> Inserir(
        [FromBody] InserirJogoRequestApiModel request)
    {
        var criado = await jogoService.InserirAsync(request.ParaModel(), request.IdsGeneros ?? new List<int>());
        return CreatedAtAction(
            nameof(ListarPorId),
            new { id = criado.Jogo.Id },
            new RetroGamesResponseApiModel<ListarPorIdJogoResponseApiModel>(criado.ParaResponsePorId()));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<RetroGamesResponseApiModel<ListarPorIdJogoResponseApiModel>>> Alterar(
        int id, [FromBody] AlterarJogoRequestApiModel request)
    {
        var alterado = await jogoService.AlterarAsync(request.ParaModel(id), request.IdsGeneros ?? new List<int>());
        return Ok(new RetroGamesResponseApiModel<ListarPorIdJogoResponseApiModel>(alterado.ParaResponsePorId()));
    }

    /// <summary>Exclusão lógica (RN03): o jogo fica inativo e sai do catálogo. HTTP 200 com o envelope.</summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<RetroGamesResponseApiModel>> Excluir(int id)
    {
        await jogoService.DesativarAsync(id);
        return Ok(new RetroGamesResponseApiModel());
    }
}
