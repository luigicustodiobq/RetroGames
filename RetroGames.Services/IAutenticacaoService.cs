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

    /// <summary>
    /// C01: cria a conta de um cliente (perfil Cliente, ativo, senha em hash) e devolve o usuário gravado.
    /// Erro 106 se o e-mail já estiver cadastrado.
    /// </summary>
    Task<Usuario> CadastrarClienteAsync(string nome, string email, string senha);
}
