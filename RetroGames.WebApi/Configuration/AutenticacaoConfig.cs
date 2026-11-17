using Microsoft.AspNetCore.Authentication.Cookies;
using RetroGames.WebApi.ApiModels;

namespace RetroGames.WebApi.Configuration;

/// <summary>
/// Sessão por cookie. O cookie é HttpOnly (o JavaScript não lê) e SameSite=Strict
/// (o navegador não o envia em requisições vindas de outro site, o que protege contra CSRF).
/// Nada de token no localStorage.
/// </summary>
public static class AutenticacaoConfig
{
    public const string NomeCookie = "retrogames.sessao";

    public static IServiceCollection AddAutenticacaoPorCookie(this IServiceCollection services)
    {
        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(opcoes =>
            {
                opcoes.Cookie.Name = NomeCookie;
                opcoes.Cookie.HttpOnly = true;
                opcoes.Cookie.SameSite = SameSiteMode.Strict;
                opcoes.ExpireTimeSpan = TimeSpan.FromHours(2);
                opcoes.SlidingExpiration = true; // renova a sessão enquanto o usuário usa a API

                // O padrão do cookie é redirecionar para uma página de login (HTTP 302).
                // Numa API, a resposta deve ser o status com o envelope em JSON.
                opcoes.Events.OnRedirectToLogin = contexto => EscreverEnvelopeAsync(
                    contexto.Response, StatusCodes.Status401Unauthorized,
                    ResponseCodes.NAO_AUTENTICADO, ResponseCodes.NAO_AUTENTICADO_MENSAGEM);

                opcoes.Events.OnRedirectToAccessDenied = contexto => EscreverEnvelopeAsync(
                    contexto.Response, StatusCodes.Status403Forbidden,
                    ResponseCodes.ACESSO_NEGADO, ResponseCodes.ACESSO_NEGADO_MENSAGEM);
            });

        return services;
    }

    private static Task EscreverEnvelopeAsync(HttpResponse resposta, int status, int responseCode, string mensagem)
    {
        resposta.StatusCode = status;
        return resposta.WriteAsJsonAsync(new RetroGamesResponseApiModel(responseCode, mensagem));
    }
}
