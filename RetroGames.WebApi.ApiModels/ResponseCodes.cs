namespace RetroGames.WebApi.ApiModels;

/// <summary>
/// Códigos gerais do campo responseCode do envelope, com as mensagens correspondentes.
/// Os erros de negócio (100 em diante) ficam no catálogo BusinessErrors, na camada Service.
/// </summary>
public static class ResponseCodes
{
    /// <summary>Operação concluída.</summary>
    public const int SUCESSO = 0;

    /// <summary>Erro interno não previsto (HTTP 500).</summary>
    public const int ERRO = 1;

    /// <summary>O corpo da requisição veio vazio (HTTP 400).</summary>
    public const int MODEL_NULL = 2;
    public const string MODEL_NULL_MENSAGEM = "O modelo veio nulo.";

    /// <summary>Campo inválido ou JSON malformado (HTTP 400); os erros por campo vão em data.</summary>
    public const int MODEL_STATE_INVALIDO = 3;
    public const string MODEL_STATE_INVALIDO_MENSAGEM = "Há um erro com o modelo enviado.";
}
