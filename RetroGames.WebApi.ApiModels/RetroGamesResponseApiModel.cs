using System.Text.Json.Serialization;

namespace RetroGames.WebApi.ApiModels;

/// <summary>
/// Envelope de todas as respostas da API, sem dados: { "responseCode": 0, "errorMessage": null }.
/// A serialização usa camelCase (padrão do ASP.NET Core).
/// </summary>
public class RetroGamesResponseApiModel
{
    /// <summary>Resposta de sucesso (responseCode 0, sem mensagem de erro).</summary>
    public RetroGamesResponseApiModel()
        : this(ResponseCodes.SUCESSO, null)
    {
    }

    public RetroGamesResponseApiModel(int responseCode, string? errorMessage)
    {
        ResponseCode = responseCode;
        ErrorMessage = errorMessage;
    }

    public int ResponseCode { get; set; }

    public string? ErrorMessage { get; set; }
}

/// <summary>Envelope com dados: { "responseCode": 0, "errorMessage": null, "data": ... }.</summary>
public class RetroGamesResponseApiModel<T> : RetroGamesResponseApiModel
{
    /// <summary>Resposta de sucesso com os dados.</summary>
    public RetroGamesResponseApiModel(T data)
    {
        Data = data;
    }

    /// <summary>Resposta de erro com dados complementares (por exemplo, erros por campo).</summary>
    public RetroGamesResponseApiModel(int responseCode, string? errorMessage, T? data)
        : base(responseCode, errorMessage)
    {
        Data = data;
    }

    /// <summary>Omitido do JSON quando nulo (respostas de erro sem dados). Sai depois de responseCode e errorMessage.</summary>
    [JsonPropertyOrder(1)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; }
}
