using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetroGames.Services;
using RetroGames.WebApi.ApiModels;
using RetroGames.WebApi.Extensions;

namespace RetroGames.WebApi.Controllers;

/// <summary>Login, logout e consulta do usuário logado (sessão por cookie).</summary>
[ApiController]
[Route("api/[controller]")]
public class AutenticacaoController : ControllerBase
{
    private readonly IAutenticacaoService autenticacaoService;

    public AutenticacaoController(IAutenticacaoService autenticacaoService)
    {
        this.autenticacaoService = autenticacaoService;
    }

    /// <summary>Confere as credenciais e, se válidas, grava o cookie de sessão.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<RetroGamesResponseApiModel<UsuarioLogadoResponseApiModel>>> Login(
        [FromBody] LoginRequestApiModel request)
    {
        var usuario = await autenticacaoService.AutenticarAsync(request.Email!, request.Senha!);
        if (usuario is null)
        {
            return Unauthorized(new RetroGamesResponseApiModel(
                ResponseCodes.CREDENCIAIS_INVALIDAS, ResponseCodes.CREDENCIAIS_INVALIDAS_MENSAGEM));
        }

        var principal = usuario.ParaClaimsPrincipal();
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return Ok(new RetroGamesResponseApiModel<UsuarioLogadoResponseApiModel>(MontarUsuarioLogado(principal)));
    }

    /// <summary>Encerra a sessão (apaga o cookie).</summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<RetroGamesResponseApiModel>> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new RetroGamesResponseApiModel());
    }

    /// <summary>Dados do usuário da sessão atual, lidos das claims do cookie.</summary>
    [Authorize]
    [HttpGet("eu")]
    public ActionResult<RetroGamesResponseApiModel<UsuarioLogadoResponseApiModel>> Eu()
    {
        return Ok(new RetroGamesResponseApiModel<UsuarioLogadoResponseApiModel>(MontarUsuarioLogado(User)));
    }

    private static UsuarioLogadoResponseApiModel MontarUsuarioLogado(ClaimsPrincipal principal)
    {
        return new UsuarioLogadoResponseApiModel
        {
            Id = principal.ObterIdUsuario() ?? 0,
            Nome = principal.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
            Email = principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            Perfil = principal.ObterPerfil() ?? string.Empty
        };
    }
}
