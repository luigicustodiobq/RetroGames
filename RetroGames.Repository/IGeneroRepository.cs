using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>Acesso a dados da tabela tb_genero (somente leitura).</summary>
public interface IGeneroRepository
{
    /// <summary>Todos os gêneros, ordenados por nome.</summary>
    Task<List<Genero>> ListarAsync();
}
