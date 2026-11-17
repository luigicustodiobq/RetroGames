using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RetroGames.WebApi.ApiModels;

namespace RetroGames.WebApi.Filters;

/// <summary>
/// Roda antes de cada action: procura um IValidator&lt;T&gt; para cada argumento recebido e,
/// se houver erros, responde HTTP 400 sem chamar a action.
/// - Corpo vazio: responseCode 2 (MODEL_NULL).
/// - Campos inválidos: responseCode 3 e, em data, as mensagens por campo (camelCase).
/// </summary>
public class FluentValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider serviceProvider;

    public FluentValidationFilter(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Parâmetro [FromBody] sem valor: o cliente não enviou o corpo.
        var parametrosDoCorpo = context.ActionDescriptor.Parameters
            .Where(parametro => parametro.BindingInfo?.BindingSource == BindingSource.Body);
        foreach (var parametro in parametrosDoCorpo)
        {
            if (!context.ActionArguments.TryGetValue(parametro.Name, out var valor) || valor is null)
            {
                context.Result = new BadRequestObjectResult(
                    new RetroGamesResponseApiModel(ResponseCodes.MODEL_NULL, ResponseCodes.MODEL_NULL_MENSAGEM));
                return;
            }
        }

        foreach (var argumento in context.ActionArguments.Values)
        {
            if (argumento is null)
            {
                continue;
            }

            // Ex.: para InserirPlataformaRequestApiModel, procura IValidator<InserirPlataformaRequestApiModel>.
            var tipoValidator = typeof(IValidator<>).MakeGenericType(argumento.GetType());
            if (serviceProvider.GetService(tipoValidator) is not IValidator validator)
            {
                continue;
            }

            var resultado = await validator.ValidateAsync(new ValidationContext<object>(argumento));
            if (!resultado.IsValid)
            {
                var errosPorCampo = resultado.Errors
                    .GroupBy(erro => JsonNamingPolicy.CamelCase.ConvertName(erro.PropertyName))
                    .ToDictionary(grupo => grupo.Key, grupo => grupo.Select(erro => erro.ErrorMessage).ToArray());

                context.Result = new BadRequestObjectResult(
                    new RetroGamesResponseApiModel<Dictionary<string, string[]>>(
                        ResponseCodes.MODEL_STATE_INVALIDO, ResponseCodes.MODEL_STATE_INVALIDO_MENSAGEM, errosPorCampo));
                return;
            }
        }

        await next();
    }
}
