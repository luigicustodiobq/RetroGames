namespace RetroGames.WebApi.ApiModels;

/// <summary>
/// Códigos gerais do campo responseCode do envelope.
/// Os erros de negócio (100 em diante) ficam no catálogo BusinessErrors, na camada Service.
/// </summary>
public static class ResponseCodes
{
    /// <summary>Operação concluída.</summary>
    public const int SUCESSO = 0;

    /// <summary>Erro interno não previsto (HTTP 500).</summary>
    public const int ERRO = 1;
}
