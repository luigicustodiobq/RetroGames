using MySqlConnector;
using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>
/// Todo o SQL da tabela tb_pedido. Os itens ficam em tb_item_pedido (ItemPedidoRepository)
/// e são combinados com os pedidos no Service. A transação da compra é aberta pelo Service.
/// </summary>
public class PedidoRepository : RepositoryBase, IPedidoRepository
{
    private const string Colunas = "co_seq_pedido, dt_pedido, vl_total, co_usuario, co_situacao_pedido";

    public PedidoRepository(RepositoryConfig config) : base(config)
    {
    }

    public async Task<int> InserirAsync(Pedido pedido)
    {
        const string sql = """
            INSERT INTO tb_pedido (dt_pedido, vl_total, co_usuario, co_situacao_pedido)
            VALUES (@dataPedido, @valorTotal, @idUsuario, @idSituacaoPedido)
            """;

        await using var conexao = await AbrirConexaoAsync();
        await using var comando = CriarComando(conexao, sql);
        AdicionarParametro(comando, "@dataPedido", pedido.DataPedido);
        AdicionarParametro(comando, "@valorTotal", pedido.ValorTotal);
        AdicionarParametro(comando, "@idUsuario", pedido.IdUsuario);
        AdicionarParametro(comando, "@idSituacaoPedido", pedido.IdSituacaoPedido);

        await ExecutarGravacaoAsync(comando);
        return (int)comando.LastInsertedId;
    }

    public async Task<List<Pedido>> ListarPorUsuarioAsync(int idUsuario)
    {
        const string sql = $"""
            SELECT {Colunas}
              FROM tb_pedido
             WHERE co_usuario = @idUsuario
             ORDER BY dt_pedido DESC, co_seq_pedido DESC
            """;

        await using var conexao = await AbrirConexaoAsync();
        await using var comando = CriarComando(conexao, sql);
        AdicionarParametro(comando, "@idUsuario", idUsuario);
        await using var leitor = await comando.ExecuteReaderAsync();

        var pedidos = new List<Pedido>();
        while (await leitor.ReadAsync())
        {
            pedidos.Add(Mapear(leitor));
        }
        return pedidos;
    }

    public async Task<Pedido?> BuscarPorIdAsync(int id)
    {
        const string sql = $"SELECT {Colunas} FROM tb_pedido WHERE co_seq_pedido = @id";

        await using var conexao = await AbrirConexaoAsync();
        await using var comando = CriarComando(conexao, sql);
        AdicionarParametro(comando, "@id", id);
        await using var leitor = await comando.ExecuteReaderAsync();

        return await leitor.ReadAsync() ? Mapear(leitor) : null;
    }

    /// <summary>Converte a linha atual do leitor num objeto Pedido, coluna por coluna.</summary>
    private static Pedido Mapear(MySqlDataReader leitor)
    {
        return new Pedido
        {
            Id = leitor.GetInt32(leitor.GetOrdinal("co_seq_pedido")),
            // dt_pedido é gravada em UTC; marcar o Kind faz a API devolver a data com "Z".
            DataPedido = DateTime.SpecifyKind(leitor.GetDateTime(leitor.GetOrdinal("dt_pedido")), DateTimeKind.Utc),
            ValorTotal = leitor.GetDecimal(leitor.GetOrdinal("vl_total")),
            IdUsuario = leitor.GetInt32(leitor.GetOrdinal("co_usuario")),
            IdSituacaoPedido = leitor.GetInt32(leitor.GetOrdinal("co_situacao_pedido"))
        };
    }
}
