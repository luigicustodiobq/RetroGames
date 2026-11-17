using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using RetroGames.Model.Constantes;
using RetroGames.Model.Model;

namespace RetroGames.WebApi.Extensions;

/// <summary>
/// Monta as claims gravadas no cookie de sessão e lê os dados do usuário logado.
/// O hash da senha nunca vai para o cookie.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>Identidade do usuário com as claims NameIdentifier, Name, Email e Role.</summary>
    public static ClaimsPrincipal ParaClaimsPrincipal(this Usuario usuario)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, NomePerfil(usuario.IdPerfil))
        };

        var identidade = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identidade);
    }

    /// <summary>Id do usuário logado, ou null se não houver sessão.</summary>
    public static int? ObterIdUsuario(this ClaimsPrincipal usuario)
    {
        var valor = usuario.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(valor, out var id) ? id : null;
    }

    /// <summary>Perfil (role) do usuário logado, ou null se não houver sessão.</summary>
    public static string? ObterPerfil(this ClaimsPrincipal usuario)
    {
        return usuario.FindFirstValue(ClaimTypes.Role);
    }

    /// <summary>Converte a chave de tb_perfil (PerfilIds) no nome usado como role (Perfis).</summary>
    public static string NomePerfil(int idPerfil)
    {
        return idPerfil == PerfilIds.Administrador ? Perfis.Administrador : Perfis.Cliente;
    }
}
