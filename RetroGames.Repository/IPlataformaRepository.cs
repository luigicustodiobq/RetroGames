using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>Acesso a dados da tabela tb_plataforma.</summary>
public interface IPlataformaRepository
{
    /// <summary>Todas as plataformas, ordenadas por nome.</summary>
    Task<List<Plataforma>> ListarAsync();

    /// <summary>A plataforma com o id informado, ou null se não existir.</summary>
    Task<Plataforma?> BuscarPorIdAsync(int id);

    /// <summary>
    /// Se já existe plataforma com o nome informado.
    /// <paramref name="idIgnorar"/> é usado na alteração, para não acusar a própria plataforma.
    /// </summary>
    Task<bool> ExisteComNomeAsync(string nome, int? idIgnorar = null);

    /// <summary>Se algum jogo (tb_jogo) usa a plataforma.</summary>
    Task<bool> PossuiJogosAsync(int idPlataforma);

    /// <summary>Grava uma nova plataforma e devolve o id gerado pelo banco.</summary>
    Task<int> InserirAsync(Plataforma plataforma);

    /// <summary>Atualiza nome, fabricante e ano de lançamento.</summary>
    Task AlterarAsync(Plataforma plataforma);

    /// <summary>Apaga a plataforma pelo id.</summary>
    Task ExcluirAsync(int id);
}
