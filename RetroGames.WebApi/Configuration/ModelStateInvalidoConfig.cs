using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RetroGames.WebApi.ApiModels;

namespace RetroGames.WebApi.Configuration;

/// <summary>
/// Resposta usada pelo [ApiController] quando o model binding falha antes da action
/// (JSON malformado ou valor de tipo errado, como "abc" num campo numérico).
/// Troca o ProblemDetails padrão pelo envelope com responseCode 3.
/// </summary>
public static class ModelStateInvalidoConfig
{
    private const string MensagemValorInvalido = "Valor inválido ou JSON malformado.";

    public static IActionResult CriarResposta(ActionContext context)
    {
        // As mensagens do model binding são técnicas e em inglês (citam tipos .NET);
        // por isso cada campo recebe uma mensagem fixa em português.
        var errosPorCampo = context.ModelState
            .Where(entrada => entrada.Value is { Errors.Count: > 0 })
            .ToDictionary(
                entrada => NormalizarCampo(entrada.Key),
                _ => new[] { MensagemValorInvalido });

        return new BadRequestObjectResult(
            new RetroGamesResponseApiModel<Dictionary<string, string[]>>(
                ResponseCodes.MODEL_STATE_INVALIDO, ResponseCodes.MODEL_STATE_INVALIDO_MENSAGEM, errosPorCampo));
    }

    /// <summary>"$.preco" ou "Preco" viram "preco"; erro sem campo (JSON quebrado) vira "corpo".</summary>
    private static string NormalizarCampo(string chave)
    {
        var campo = chave.StartsWith("$.") ? chave[2..] : chave.TrimStart('$');
        return string.IsNullOrEmpty(campo) ? "corpo" : JsonNamingPolicy.CamelCase.ConvertName(campo);
    }
}
