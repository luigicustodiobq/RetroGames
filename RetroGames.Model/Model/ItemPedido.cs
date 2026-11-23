namespace RetroGames.Model.Model;

/// <summary>Jogo comprado em um pedido. Tabela tb_item_pedido.</summary>
public class ItemPedido
{
    // co_seq_item_pedido
    public int Id { get; set; }

    // qt_item
    public int Quantidade { get; set; }

    // vl_unitario (preço do jogo no momento da compra)
    public decimal ValorUnitario { get; set; }

    // co_pedido
    public int IdPedido { get; set; }

    // co_jogo
    public int IdJogo { get; set; }
}
