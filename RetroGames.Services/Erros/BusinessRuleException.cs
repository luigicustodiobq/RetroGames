namespace RetroGames.Services.Erros;

/// <summary>
/// Lançada pelo Service quando uma regra de negócio é violada.
/// O erro do catálogo fica em <see cref="Erro"/>; o tratamento global da API
/// usa Erro.Status e Erro.Code para montar a resposta.
/// </summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(BusinessError erro, Exception? excecaoInterna = null)
        : base(erro.Message, excecaoInterna)
    {
        Erro = erro;
    }

    public BusinessError Erro { get; }
}
