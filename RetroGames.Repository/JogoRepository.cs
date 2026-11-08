using MySqlConnector;
using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>
/// Todo o SQL da tabela tb_jogo. Nenhuma consulta junta tabelas: plataforma, condição
/// e gêneros são combinados no Service. Nenhum método apaga jogo (exclusão lógica).
/// </summary>
public class JogoRepository : RepositoryBase, IJogoRepository
{
    private const string Colunas = """
        co_seq_jogo, no_jogo, ds_jogo, nu_ano_lancamento, vl_preco, qt_estoque,
        st_ativo, dt_cadastro, co_plataforma, co_condicao_jogo
        """;

    public JogoRepository(RepositoryConfig config) : base(config)
    {
    }

    public async Task<(List<Jogo> Jogos, int Total)> ListarAsync(
        string? busca,
        int? idPlataforma,
        IReadOnlyCollection<int>? idsJogos,
        bool incluirInativos,
        int pagina,
        int tamanhoPagina)
    {
        // "IN ()" é SQL inválido: lista vazia quer dizer que nenhum jogo atende ao filtro.
        if (idsJogos is { Count: 0 })
        {
            return (new List<Jogo>(), 0);
        }

        // Só trechos fixos de SQL entram no WHERE; os valores vão sempre como parâmetro.
        var condicoes = new List<string>();
        if (!incluirInativos)
        {
            condicoes.Add("st_ativo = TRUE");
        }
        if (!string.IsNullOrWhiteSpace(busca))
        {
            condicoes.Add("no_jogo LIKE CONCAT('%', @busca, '%')");
        }
        if (idPlataforma.HasValue)
        {
            condicoes.Add("co_plataforma = @idPlataforma");
        }
        var nomesParametrosIds = new List<string>();
        if (idsJogos is not null)
        {
            nomesParametrosIds = idsJogos.Select((_, indice) => $"@idJogo{indice}").ToList();
            condicoes.Add($"co_seq_jogo IN ({string.Join(", ", nomesParametrosIds)})");
        }

        var where = condicoes.Count > 0 ? "WHERE " + string.Join(" AND ", condicoes) : string.Empty;

        var sqlTotal = $"SELECT COUNT(*) FROM tb_jogo {where}";
        var sqlPagina = $"""
            SELECT {Colunas}
              FROM tb_jogo
              {where}
             ORDER BY no_jogo, co_seq_jogo
             LIMIT @limite OFFSET @deslocamento
            """;

        // Mesmos parâmetros de filtro nos dois comandos, para a contagem bater com a listagem.
        void AdicionarFiltros(MySqlCommand comando)
        {
            if (!string.IsNullOrWhiteSpace(busca))
            {
                AdicionarParametro(comando, "@busca", busca.Trim());
            }
            if (idPlataforma.HasValue)
            {
                AdicionarParametro(comando, "@idPlataforma", idPlataforma.Value);
            }
            if (idsJogos is not null)
            {
                var indice = 0;
                foreach (var idJogo in idsJogos)
                {
                    AdicionarParametro(comando, nomesParametrosIds[indice], idJogo);
                    indice++;
                }
            }
        }

        await using var conexao = await AbrirConexaoAsync();

        int total;
        await using (var comandoTotal = CriarComando(conexao, sqlTotal))
        {
            AdicionarFiltros(comandoTotal);
            total = Convert.ToInt32(await comandoTotal.ExecuteScalarAsync());
        }

        var jogos = new List<Jogo>();
        await using (var comandoPagina = CriarComando(conexao, sqlPagina))
        {
            AdicionarFiltros(comandoPagina);
            AdicionarParametro(comandoPagina, "@limite", tamanhoPagina);
            AdicionarParametro(comandoPagina, "@deslocamento", (pagina - 1) * tamanhoPagina);

            await using var leitor = await comandoPagina.ExecuteReaderAsync();
            while (await leitor.ReadAsync())
            {
                jogos.Add(Mapear(leitor));
            }
        }

        return (jogos, total);
    }

    public async Task<Jogo?> BuscarPorIdAsync(int id)
    {
        const string sql = $"SELECT {Colunas} FROM tb_jogo WHERE co_seq_jogo = @id";

        await using var conexao = await AbrirConexaoAsync();
        await using var comando = CriarComando(conexao, sql);
        AdicionarParametro(comando, "@id", id);
        await using var leitor = await comando.ExecuteReaderAsync();

        return await leitor.ReadAsync() ? Mapear(leitor) : null;
    }

    public async Task<int> InserirAsync(Jogo jogo)
    {
        const string sql = """
            INSERT INTO tb_jogo
                (no_jogo, ds_jogo, nu_ano_lancamento, vl_preco, qt_estoque, co_plataforma, co_condicao_jogo)
            VALUES
                (@nome, @descricao, @anoLancamento, @preco, @estoque, @idPlataforma, @idCondicaoJogo)
            """;

        await using var conexao = await AbrirConexaoAsync();
        await using var comando = CriarComando(conexao, sql);
        AdicionarParametrosDeDados(comando, jogo);

        await ExecutarGravacaoAsync(comando);
        return (int)comando.LastInsertedId;
    }

    public async Task AlterarAsync(Jogo jogo)
    {
        const string sql = """
            UPDATE tb_jogo
               SET no_jogo = @nome,
                   ds_jogo = @descricao,
                   nu_ano_lancamento = @anoLancamento,
                   vl_preco = @preco,
                   qt_estoque = @estoque,
                   co_plataforma = @idPlataforma,
                   co_condicao_jogo = @idCondicaoJogo
             WHERE co_seq_jogo = @id
            """;

        await using var conexao = await AbrirConexaoAsync();
        await using var comando = CriarComando(conexao, sql);
        AdicionarParametrosDeDados(comando, jogo);
        AdicionarParametro(comando, "@id", jogo.Id);

        await ExecutarGravacaoAsync(comando);
    }

    public async Task DesativarAsync(int id)
    {
        const string sql = "UPDATE tb_jogo SET st_ativo = FALSE WHERE co_seq_jogo = @id";

        await using var conexao = await AbrirConexaoAsync();
        await using var comando = CriarComando(conexao, sql);
        AdicionarParametro(comando, "@id", id);

        await ExecutarGravacaoAsync(comando);
    }

    /// <summary>Parâmetros comuns a inserir e alterar.</summary>
    private void AdicionarParametrosDeDados(MySqlCommand comando, Jogo jogo)
    {
        AdicionarParametro(comando, "@nome", jogo.Nome);
        AdicionarParametro(comando, "@descricao", jogo.Descricao); // null vira NULL no banco
        AdicionarParametro(comando, "@anoLancamento", jogo.AnoLancamento);
        AdicionarParametro(comando, "@preco", jogo.Preco);
        AdicionarParametro(comando, "@estoque", jogo.Estoque);
        AdicionarParametro(comando, "@idPlataforma", jogo.IdPlataforma);
        AdicionarParametro(comando, "@idCondicaoJogo", jogo.IdCondicaoJogo);
    }

    /// <summary>Converte a linha atual do leitor num objeto Jogo, coluna por coluna.</summary>
    private static Jogo Mapear(MySqlDataReader leitor)
    {
        var ordinalDescricao = leitor.GetOrdinal("ds_jogo");

        return new Jogo
        {
            Id = leitor.GetInt32(leitor.GetOrdinal("co_seq_jogo")),
            Nome = leitor.GetString(leitor.GetOrdinal("no_jogo")),
            Descricao = leitor.IsDBNull(ordinalDescricao) ? null : leitor.GetString(ordinalDescricao),
            AnoLancamento = leitor.GetInt32(leitor.GetOrdinal("nu_ano_lancamento")),
            Preco = leitor.GetDecimal(leitor.GetOrdinal("vl_preco")),
            Estoque = leitor.GetInt32(leitor.GetOrdinal("qt_estoque")),
            Ativo = leitor.GetBoolean(leitor.GetOrdinal("st_ativo")),
            DataCadastro = leitor.GetDateTime(leitor.GetOrdinal("dt_cadastro")),
            IdPlataforma = leitor.GetInt32(leitor.GetOrdinal("co_plataforma")),
            IdCondicaoJogo = leitor.GetInt32(leitor.GetOrdinal("co_condicao_jogo"))
        };
    }
}
