using MySqlConnector;

namespace RetroGames.Repository;

/// <summary>
/// Base de todos os repositórios: concentra o que se repete em cada acesso ao banco
/// (abrir conexão, criar comando, passar parâmetros e executar gravações).
/// Não contém SQL de nenhuma tabela.
/// </summary>
/// <remarks>
/// Quem chama é responsável por liberar os objetos devolvidos com <c>await using</c>,
/// para que conexão, comando e leitor sejam fechados mesmo em caso de erro.
/// O Repository não abre transações: se o Service tiver aberto um TransactionScope,
/// a conexão do MySqlConnector participa dele automaticamente ao ser aberta.
/// </remarks>
public abstract class RepositoryBase
{
    private readonly RepositoryConfig _config;

    protected RepositoryBase(RepositoryConfig config)
    {
        _config = config;
    }

    /// <summary>Cria e abre uma conexão com o banco.</summary>
    protected async Task<MySqlConnection> AbrirConexaoAsync()
    {
        var conexao = new MySqlConnection(_config.ConnectionString);
        try
        {
            await conexao.OpenAsync();
            return conexao;
        }
        catch
        {
            await conexao.DisposeAsync();
            throw;
        }
    }

    /// <summary>Cria um comando com o texto SQL informado, ligado à conexão.</summary>
    protected MySqlCommand CriarComando(MySqlConnection conexao, string sql)
    {
        return new MySqlCommand(sql, conexao);
    }

    /// <summary>
    /// Adiciona um parâmetro ao comando. O valor nunca é concatenado no SQL.
    /// Valores nulos são enviados como DBNull.Value (NULL no banco).
    /// </summary>
    protected void AdicionarParametro(MySqlCommand comando, string nome, object? valor)
    {
        comando.Parameters.AddWithValue(nome, valor ?? DBNull.Value);
    }

    /// <summary>
    /// Executa uma gravação (INSERT ou UPDATE) e devolve o número de linhas afetadas.
    /// Único lugar em que o erro 1062 (UNIQUE) é convertido em <see cref="RegistroDuplicadoException"/>.
    /// Após um INSERT, o id gerado fica disponível em <c>comando.LastInsertedId</c>.
    /// </summary>
    protected async Task<int> ExecutarGravacaoAsync(MySqlCommand comando)
    {
        try
        {
            return await comando.ExecuteNonQueryAsync();
        }
        catch (MySqlException ex) when (ex.Number == (int)MySqlErrorCode.DuplicateKeyEntry) // 1062
        {
            throw new RegistroDuplicadoException("Já existe um registro com este valor.", ex);
        }
    }
}
