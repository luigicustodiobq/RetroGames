using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>Acesso a dados da tabela de domínio tb_condicao_jogo (somente leitura).</summary>
public interface ICondicaoJogoRepository
{
    /// <summary>Todas as condições de jogo, na ordem das chaves fixas.</summary>
    Task<List<CondicaoJogo>> ListarAsync();
}
