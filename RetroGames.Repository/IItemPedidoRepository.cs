using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>Acesso a dados da tabela tb_item_pedido.</summary>
public interface IItemPedidoRepository
{
    /// <summary>Grava um item de pedido e devolve o id gerado pelo banco.</summary>
    Task<int> InserirAsync(ItemPedido item);

    /// <summary>Os itens de uma lista de pedidos, numa única consulta.</summary>
    Task<List<ItemPedido>> ListarPorPedidosAsync(IReadOnlyCollection<int> idsPedidos);
}
