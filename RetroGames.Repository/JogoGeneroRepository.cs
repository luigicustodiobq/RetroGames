using MySqlConnector;
using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>
/// Todo o SQL da tabela tb_jogo_genero. Quando o Service grava um jogo e seus vínculos,
/// é ele quem abre a transação (TransactionScope); este Repository só executa os comandos.
/// </summary>
public class JogoGeneroRepository : RepositoryBase, IJogoGeneroRepository
{
    public JogoGeneroRepository(RepositoryConfig config) : base(config)
    {
    }

    public async Task<List<JogoGenero>> ListarTodosAsync()
    {
        const string sql = "SELECT co_jogo, co_genero FROM tb_jogo_genero ORDER BY co_jogo, co_genero";

        await using var conexao = await AbrirConexaoAsync();
        await using var comando = CriarComando(conexao, sql);
        return await LerVinculosAsync(comando);
    }

    public async Task<List<JogoGenero>> ListarPorJogosAsync(IReadOnlyCollection<int> idsJogos)
    {
        // "IN ()" é SQL inválido: sem jogos, não há vínculos.
        if (idsJogos.Count == 0)
        {
            return new List<JogoGenero>();
        }

        // Os nomes dos parâmetros são gerados (@idJogo0, @idJogo1...); os valores vão como parâmetro.
        var nomesParametros = idsJogos.Select((_, indice) => $"@idJogo{indice}").ToList();
        var sql = $"""
            SELECT co_jogo, co_genero
              FROM tb_jogo_genero
             WHERE co_jogo IN ({string.Join(", ", nomesParametros)})
             ORDER BY co_jogo, co_genero
            """;

        await using var conexao = await AbrirConexaoAsync();
        await using var comando = CriarComando(conexao, sql);
        var indiceParametro = 0;
        foreach (var idJogo in idsJogos)
        {
            AdicionarParametro(comando, nomesParametros[indiceParametro], idJogo);
            indiceParametro++;
        }
        return await LerVinculosAsync(comando);
    }

    public async Task InserirAsync(int idJogo, IReadOnlyCollection<int> idsGeneros)
    {
        if (idsGeneros.Count == 0)
        {
            return;
        }

        // Um único INSERT com várias linhas: (@idJogo, @idGenero0), (@idJogo, @idGenero1)...
        var nomesParametros = idsGeneros.Select((_, indice) => $"@idGenero{indice}").ToList();
        var linhas = nomesParametros.Select(nome => $"(@idJogo, {nome})");
        var sql = $"INSERT INTO tb_jogo_genero (co_jogo, co_genero) VALUES {string.Join(", ", linhas)}";

        await using var conexao = await AbrirConexaoAsync();
        await using var comando = CriarComando(conexao, sql);
        AdicionarParametro(comando, "@idJogo", idJogo);
        var indiceParametro = 0;
        foreach (var idGenero in idsGeneros)
        {
            AdicionarParametro(comando, nomesParametros[indiceParametro], idGenero);
            indiceParametro++;
        }

        await ExecutarGravacaoAsync(comando);
    }

    public async Task RemoverPorJogoAsync(int idJogo)
    {
        const string sql = "DELETE FROM tb_jogo_genero WHERE co_jogo = @idJogo";

        await using var conexao = await AbrirConexaoAsync();
        await using var comando = CriarComando(conexao, sql);
        AdicionarParametro(comando, "@idJogo", idJogo);

        await comando.ExecuteNonQueryAsync();
    }

    /// <summary>Executa a consulta e converte cada linha num objeto JogoGenero.</summary>
    private static async Task<List<JogoGenero>> LerVinculosAsync(MySqlCommand comando)
    {
        await using var leitor = await comando.ExecuteReaderAsync();

        var vinculos = new List<JogoGenero>();
        while (await leitor.ReadAsync())
        {
            vinculos.Add(new JogoGenero
            {
                IdJogo = leitor.GetInt32(leitor.GetOrdinal("co_jogo")),
                IdGenero = leitor.GetInt32(leitor.GetOrdinal("co_genero"))
            });
        }
        return vinculos;
    }
}
