using RetroGames.Model.Model;

namespace RetroGames.Services;

/// <summary>Consulta das condições de jogo (somente leitura).</summary>
public interface ICondicaoJogoService
{
    Task<List<CondicaoJogo>> ListarAsync();
}
