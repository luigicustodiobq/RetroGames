namespace RetroGames.Services;

/// <summary>
/// Quem está fazendo a requisição atual. A interface fica no Service (que não conhece HTTP);
/// a implementação fica na API, que lê o cookie de sessão.
/// </summary>
public interface IUsuarioAtualService
{
    bool EstaAutenticado { get; }

    /// <summary>Id do usuário logado, ou null para visitante.</summary>
    int? IdUsuario { get; }

    bool EhAdministrador { get; }
}
