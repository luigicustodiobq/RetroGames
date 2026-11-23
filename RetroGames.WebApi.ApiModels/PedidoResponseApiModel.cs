namespace RetroGames.WebApi.ApiModels;

/// <summary>Pedido devolvido pela API (finalização e Meus pedidos).</summary>
public class PedidoResponseApiModel
{
    public int Id { get; set; }

    /// <summary>Data do pedido em UTC (ISO 8601).</summary>
    public DateTime DataPedido { get; set; }

    public decimal ValorTotal { get; set; }

    public int IdSituacaoPedido { get; set; }

    /// <summary>"Pendente", "Pago", "Enviado" ou "Cancelado".</summary>
    public string DescricaoSituacao { get; set; } = string.Empty;

    public List<ItemPedidoResponseApiModel> Itens { get; set; } = new();
}

/// <summary>Jogo comprado, com o preço unitário pago no momento da compra.</summary>
public class ItemPedidoResponseApiModel
{
    public int IdJogo { get; set; }

    public string NomeJogo { get; set; } = string.Empty;

    public int Quantidade { get; set; }

    public decimal ValorUnitario { get; set; }

    public decimal Subtotal { get; set; }
}
