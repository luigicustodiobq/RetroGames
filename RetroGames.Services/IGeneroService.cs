using RetroGames.Model.Model;

namespace RetroGames.Services;

/// <summary>Consulta de gêneros (somente leitura).</summary>
public interface IGeneroService
{
    Task<List<Genero>> ListarAsync();
}
