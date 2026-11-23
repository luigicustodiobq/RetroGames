using RetroGames.Model.Model;

namespace RetroGames.Services.ServiceModels;

/// <summary>
/// Um pedido com a descrição da situação e os itens (com o nome do jogo),
/// montado no Service com LINQ (o Repository lê uma tabela por consulta).
/// </summary>
public class PedidoDetalheServiceModel
{
    public Pedido Pedido { get; set; } = new();

    public string DescricaoSituacao { get; set; } = string.Empty;

    public List<ItemPedidoDetalheServiceModel> Itens { get; set; } = new();
}

/// <summary>Item de um pedido com o nome do jogo comprado.</summary>
public class ItemPedidoDetalheServiceModel
{
    public ItemPedido ItemPedido { get; set; } = new();

    public string NomeJogo { get; set; } = string.Empty;
}
