using Microsoft.AspNetCore.Mvc;
using RetroGames.Services;
using RetroGames.WebApi.ApiModels;
using RetroGames.WebApi.Mappers;

namespace RetroGames.WebApi.Controllers;

/// <summary>
/// Rotas de plataformas. Sem try/catch: os erros de negócio são convertidos
/// em resposta pelo tratamento global de exceções.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PlataformaController : ControllerBase
{
    private readonly IPlataformaService plataformaService;

    public PlataformaController(IPlataformaService plataformaService)
    {
        this.plataformaService = plataformaService;
    }

    [HttpGet]
    public async Task<ActionResult<RetroGamesResponseApiModel<List<ListarPlataformaResponseApiModel>>>> Listar()
    {
        var plataformas = await plataformaService.ListarAsync();
        var resposta = plataformas.Select(plataforma => plataforma.ParaResponse()).ToList();
        return Ok(new RetroGamesResponseApiModel<List<ListarPlataformaResponseApiModel>>(resposta));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RetroGamesResponseApiModel<ListarPlataformaResponseApiModel>>> ListarPorId(int id)
    {
        var plataforma = await plataformaService.BuscarPorIdAsync(id);
        return Ok(new RetroGamesResponseApiModel<ListarPlataformaResponseApiModel>(plataforma.ParaResponse()));
    }

    /// <summary>HTTP 201 com o header Location apontando para GET /api/Plataforma/{id}.</summary>
    [HttpPost]
    public async Task<ActionResult<RetroGamesResponseApiModel<ListarPlataformaResponseApiModel>>> Inserir(
        [FromBody] InserirPlataformaRequestApiModel request)
    {
        var criada = await plataformaService.InserirAsync(request.ParaModel());
        return CreatedAtAction(
            nameof(ListarPorId),
            new { id = criada.Id },
            new RetroGamesResponseApiModel<ListarPlataformaResponseApiModel>(criada.ParaResponse()));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<RetroGamesResponseApiModel<ListarPlataformaResponseApiModel>>> Alterar(
        int id, [FromBody] AlterarPlataformaRequestApiModel request)
    {
        var alterada = await plataformaService.AlterarAsync(request.ParaModel(id));
        return Ok(new RetroGamesResponseApiModel<ListarPlataformaResponseApiModel>(alterada.ParaResponse()));
    }

    /// <summary>HTTP 200 com o envelope (a API não usa 204).</summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<RetroGamesResponseApiModel>> Excluir(int id)
    {
        await plataformaService.ExcluirAsync(id);
        return Ok(new RetroGamesResponseApiModel());
    }
}
