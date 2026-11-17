using RetroGames.Model.Model;
using RetroGames.Repository;

namespace RetroGames.Services;

/// <summary>Consulta de gêneros, usando repositorio.Genero.</summary>
public class GeneroService : IGeneroService
{
    private readonly IRepositorySet repositorio;

    public GeneroService(IRepositorySet repositorio)
    {
        this.repositorio = repositorio;
    }

    public Task<List<Genero>> ListarAsync()
    {
        return repositorio.Genero.ListarAsync();
    }
}
