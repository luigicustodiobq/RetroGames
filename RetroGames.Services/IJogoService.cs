using RetroGames.Model.Model;
using RetroGames.Services.ServiceModels;

namespace RetroGames.Services;

/// <summary>Regras de negócio de jogos.</summary>
public interface IJogoService
{
    /// <summary>Página de jogos (com plataforma, condição e gêneros) e o total encontrado.</summary>
    Task<PaginaServiceModel<JogoDetalheServiceModel>> ListarAsync(FiltroJogoServiceModel filtro);

    /// <summary>O jogo com seus detalhes; erro 103 se não existir.</summary>
    Task<JogoDetalheServiceModel> BuscarPorIdAsync(int id);

    /// <summary>Grava o jogo e seus gêneros numa transação; erros 100, 104 e 105.</summary>
    Task<JogoDetalheServiceModel> InserirAsync(Jogo jogo, IReadOnlyCollection<int> idsGeneros);

    /// <summary>Altera o jogo e regrava seus gêneros numa transação; erros 103, 100, 104 e 105.</summary>
    Task<JogoDetalheServiceModel> AlterarAsync(Jogo jogo, IReadOnlyCollection<int> idsGeneros);

    /// <summary>Exclusão lógica (st_ativo = FALSE); erro 103 se não existir.</summary>
    Task DesativarAsync(int id);
}
