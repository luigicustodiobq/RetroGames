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

    /// <summary>Rota protegida acessada sem sessão (HTTP 401).</summary>
    public const int NAO_AUTENTICADO = 4;
    public const string NAO_AUTENTICADO_MENSAGEM = "Faça login para continuar.";

    /// <summary>Usuário logado sem o perfil exigido (HTTP 403).</summary>
    public const int ACESSO_NEGADO = 5;
    public const string ACESSO_NEGADO_MENSAGEM = "Você não tem permissão para esta operação.";

    /// <summary>E-mail ou senha não conferem, ou usuário inativo (HTTP 401).</summary>
    public const int CREDENCIAIS_INVALIDAS = 6;
    public const string CREDENCIAIS_INVALIDAS_MENSAGEM = "E-mail ou senha inválidos.";
}
