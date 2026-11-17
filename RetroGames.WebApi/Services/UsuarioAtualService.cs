using RetroGames.Model.Constantes;
using RetroGames.Services;
using RetroGames.WebApi.Extensions;

namespace RetroGames.WebApi.Services;

/// <summary>Lê o usuário da requisição atual a partir das claims do cookie de sessão.</summary>
public class UsuarioAtualService : IUsuarioAtualService
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public UsuarioAtualService(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public bool EstaAutenticado =>
        httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public int? IdUsuario =>
        EstaAutenticado ? httpContextAccessor.HttpContext!.User.ObterIdUsuario() : null;

    public bool EhAdministrador =>
        EstaAutenticado && httpContextAccessor.HttpContext!.User.IsInRole(Perfis.Administrador);
}
