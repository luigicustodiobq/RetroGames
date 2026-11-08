using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>Acesso a dados da tabela tb_jogo.</summary>
public interface IJogoRepository
{
    /// <summary>
    /// Os jogos de uma página e o total encontrado com o mesmo filtro.
    /// Cada filtro só é aplicado quando informado (não nulo).
    /// </summary>
    /// <param name="busca">Parte do nome do jogo (LIKE).</param>
    /// <param name="idPlataforma">Plataforma do jogo.</param>
    /// <param name="idsJogos">
    /// Lista de ids de jogos (por exemplo, os jogos de um gênero, resolvidos no Service).
    /// Lista vazia significa "nenhum jogo atende" e devolve uma página vazia.
    /// </param>
    /// <param name="incluirInativos">Quando falso, traz só jogos com st_ativo = TRUE.</param>
    /// <param name="pagina">Número da página, começando em 1.</param>
    /// <param name="tamanhoPagina">Quantidade de jogos por página.</param>
    Task<(List<Jogo> Jogos, int Total)> ListarAsync(
        string? busca,
        int? idPlataforma,
        IReadOnlyCollection<int>? idsJogos,
        bool incluirInativos,
        int pagina,
        int tamanhoPagina);

    /// <summary>O jogo com o id informado (ativo ou não), ou null se não existir.</summary>
    Task<Jogo?> BuscarPorIdAsync(int id);

    /// <summary>
    /// Grava um novo jogo e devolve o id gerado pelo banco.
    /// st_ativo e dt_cadastro recebem os valores padrão da tabela (TRUE e a data atual).
    /// </summary>
    Task<int> InserirAsync(Jogo jogo);

    /// <summary>
    /// Atualiza nome, descrição, ano, preço, estoque, plataforma e condição.
    /// Não altera st_ativo nem dt_cadastro.
    /// </summary>
    Task AlterarAsync(Jogo jogo);

    /// <summary>Exclusão lógica: marca o jogo como inativo (st_ativo = FALSE).</summary>
    Task DesativarAsync(int id);
}
