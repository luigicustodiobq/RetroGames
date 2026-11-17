using RetroGames.Model.Model;

namespace RetroGames.Services;

/// <summary>Regras de negócio de plataformas.</summary>
public interface IPlataformaService
{
    Task<List<Plataforma>> ListarAsync();

    /// <summary>A plataforma do id; erro 100 se não existir.</summary>
    Task<Plataforma> BuscarPorIdAsync(int id);

    /// <summary>Grava e devolve a plataforma com o Id gerado; erro 101 se o nome já existir.</summary>
    Task<Plataforma> InserirAsync(Plataforma plataforma);

    /// <summary>Altera e devolve a plataforma; erros 100 e 101.</summary>
    Task<Plataforma> AlterarAsync(Plataforma plataforma);

    /// <summary>Exclui a plataforma; erros 100 e 102 (possui jogos).</summary>
    Task ExcluirAsync(int id);
}
