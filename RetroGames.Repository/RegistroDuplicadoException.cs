namespace RetroGames.Repository;

/// <summary>
/// Lançada pelo Repository quando o MySQL recusa um valor repetido numa coluna UNIQUE
/// (erro 1062 - DuplicateKeyEntry). O Service converte esta exceção no erro de negócio
/// correspondente (por exemplo, HTTP 409 / responseCode 8 para nome de plataforma duplicado).
/// </summary>
public class RegistroDuplicadoException : Exception
{
    public RegistroDuplicadoException(string mensagem, Exception excecaoInterna)
        : base(mensagem, excecaoInterna)
    {
    }
}
