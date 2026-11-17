using RetroGames.Model.Constantes;

namespace RetroGames.WebApi.Configuration;

/// <summary>
/// Políticas de autorização. Sem login, a rota protegida responde 401 (responseCode 4);
/// logado sem o perfil exigido, 403 (responseCode 5) — ver AutenticacaoConfig.
/// </summary>
public static class AutorizacaoConfig
{
    public static IServiceCollection AddAutorizacao(this IServiceCollection services)
    {
        services.AddAuthorization(opcoes =>
            opcoes.AddPolicy(Politicas.SomenteAdministrador, politica => politica.RequireRole(Perfis.Administrador)));

        return services;
    }
}
