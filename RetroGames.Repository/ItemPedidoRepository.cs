using MySqlConnector;
using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>Todo o SQL da tabela tb_item_pedido.</summary>
public class ItemPedidoRepository : RepositoryBase, IItemPedidoRepository
{
    public ItemPedidoRepository(RepositoryConfig config) : base(config)
    {
    }

    public async Task<int> InserirAsync(ItemPedido item)
    {
        const string sql = """
            INSERT INTO tb_item_pedido (qt_item, vl_unitario, co_pedido, co_jogo)
            VALUES (@quantidade, @valorUnitario, @idPedido, @idJogo)
            """;

        await using var conexao = await AbrirConexaoAsync();
        await using var comando = CriarComando(conexao, sql);
        AdicionarParametro(comando, "@quantidade", item.Quantidade);
        AdicionarParametro(comando, "@valorUnitario", item.ValorUnitario);
        AdicionarParametro(comando, "@idPedido", item.IdPedido);
        AdicionarParametro(comando, "@idJogo", item.IdJogo);

        await ExecutarGravacaoAsync(comando);
        return (int)comando.LastInsertedId;
    }

    public async Task<List<ItemPedido>> ListarPorPedidosAsync(IReadOnlyCollection<int> idsPedidos)
    {
        // "IN ()" é SQL inválido: sem pedidos, não há itens.
        if (idsPedidos.Count == 0)
        {
            return new List<ItemPedido>();
        }

        var nomesParametros = idsPedidos.Select((_, indice) => $"@idPedido{indice}").ToList();
        var sql = $"""
            SELECT co_seq_item_pedido, qt_item, vl_unitario, co_pedido, co_jogo
              FROM tb_item_pedido
             WHERE co_pedido IN ({string.Join(", ", nomesParametros)})
             ORDER BY co_pedido, co_seq_item_pedido
            """;

        await using var conexao = await AbrirConexaoAsync();
        await using var comando = CriarComando(conexao, sql);
        var indiceParametro = 0;
        foreach (var idPedido in idsPedidos)
        {
            AdicionarParametro(comando, nomesParametros[indiceParametro], idPedido);
            indiceParametro++;
        }

        await using var leitor = await comando.ExecuteReaderAsync();
        var itens = new List<ItemPedido>();
        while (await leitor.ReadAsync())
        {
            itens.Add(Mapear(leitor));
        }
        return itens;
    }

    /// <summary>Converte a linha atual do leitor num objeto ItemPedido, coluna por coluna.</summary>
    private static ItemPedido Mapear(MySqlDataReader leitor)
    {
        return new ItemPedido
        {
            Id = leitor.GetInt32(leitor.GetOrdinal("co_seq_item_pedido")),
            Quantidade = leitor.GetInt32(leitor.GetOrdinal("qt_item")),
            ValorUnitario = leitor.GetDecimal(leitor.GetOrdinal("vl_unitario")),
            IdPedido = leitor.GetInt32(leitor.GetOrdinal("co_pedido")),
            IdJogo = leitor.GetInt32(leitor.GetOrdinal("co_jogo"))
        };
    }
}
