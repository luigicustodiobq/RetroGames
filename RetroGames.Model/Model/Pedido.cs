namespace RetroGames.Model.Model;

/// <summary>Pedido de compra de um cliente. Tabela tb_pedido.</summary>
public class Pedido
{
    // co_seq_pedido
    public int Id { get; set; }

    // dt_pedido (gravada em UTC pelo Service)
    public DateTime DataPedido { get; set; }

    // vl_total (calculado no Service com os preços do banco)
    public decimal ValorTotal { get; set; }

    // co_usuario
    public int IdUsuario { get; set; }

    // co_situacao_pedido
    public int IdSituacaoPedido { get; set; }
}
