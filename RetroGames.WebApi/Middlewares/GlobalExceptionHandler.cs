using Microsoft.AspNetCore.Diagnostics;
using RetroGames.Services.Erros;
using RetroGames.WebApi.ApiModels;

namespace RetroGames.WebApi.Middlewares;

/// <summary>
/// Converte qualquer exceção não tratada na resposta padrão (envelope).
/// - BusinessRuleException: status e responseCode do catálogo BusinessErrors.
/// - Qualquer outra: registra no log e devolve HTTP 500 com mensagem genérica,
///   sem expor SQL nem stack trace ao cliente.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private const string MensagemErroInterno = "Ocorreu um erro interno no servidor.";

    private readonly ILogger<GlobalExceptionHandler> logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        this.logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is BusinessRuleException ex)
        {
            httpContext.Response.StatusCode = (int)ex.Erro.Status;
            await httpContext.Response.WriteAsJsonAsync(
                new RetroGamesResponseApiModel(ex.Erro.Code, ex.Message), cancellationToken);
            return true;
        }

        logger.LogError(exception, "Erro não tratado em {Metodo} {Caminho}",
            httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(
            new RetroGamesResponseApiModel(ResponseCodes.ERRO, MensagemErroInterno), cancellationToken);
        return true;
    }
}
