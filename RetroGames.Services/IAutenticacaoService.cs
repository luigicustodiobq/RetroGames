using RetroGames.Model.Model;

namespace RetroGames.Services;

/// <summary>Conferência de credenciais e criação do administrador inicial.</summary>
public interface IAutenticacaoService
{
    /// <summary>
    /// O usuário, se e-mail e senha conferem e ele está ativo; senão, null.
    /// Não informa qual dos dois falhou, para não revelar quais e-mails existem.
    /// </summary>
    Task<Usuario?> AutenticarAsync(string email, string senha);

    /// <summary>Cria o administrador inicial se ainda não houver nenhum administrador.</summary>
    Task GarantirAdministradorInicialAsync(string email, string senha);
}
