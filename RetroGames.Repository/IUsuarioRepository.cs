using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>Acesso a dados da tabela tb_usuario.</summary>
public interface IUsuarioRepository
{
    /// <summary>O usuário com o e-mail informado, ou null se não existir.</summary>
    Task<Usuario?> BuscarPorEmailAsync(string email);

    /// <summary>O usuário com o id informado, ou null se não existir.</summary>
    Task<Usuario?> BuscarPorIdAsync(int id);

    /// <summary>Se existe algum usuário com o perfil Administrador (co_perfil = 1).</summary>
    Task<bool> ExisteAdministradorAsync();

    /// <summary>
    /// Grava um novo usuário e devolve o id gerado pelo banco.
    /// st_ativo e dt_cadastro recebem os valores padrão da tabela.
    /// </summary>
    Task<int> InserirAsync(Usuario usuario);
}
