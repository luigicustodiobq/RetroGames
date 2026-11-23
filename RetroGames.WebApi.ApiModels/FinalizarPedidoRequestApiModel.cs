namespace RetroGames.WebApi.ApiModels;

/// <summary>
/// Corpo de POST /api/Pedido (C04). Só jogo e quantidade: o usuário vem da sessão
/// e o preço vem do banco.
/// </summary>
public class FinalizarPedidoRequestApiModel
{
    public List<ItemPedidoRequestApiModel>? Itens { get; set; }
}

/// <summary>Um item do carrinho enviado na finalização da compra.</summary>
public class ItemPedidoRequestApiModel
{
    public int IdJogo { get; set; }

    public int Quantidade { get; set; }
}
