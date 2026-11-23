using RetroGames.Services.ServiceModels;
using RetroGames.WebApi.ApiModels;

namespace RetroGames.WebApi.Mappers;

/// <summary>Conversões entre os modelos de pedido do Service e os ApiModels.</summary>
public static class PedidoMapper
{
    public static List<ItemCompraServiceModel> ParaItensCompra(this FinalizarPedidoRequestApiModel request)
    {
        return (request.Itens ?? new List<ItemPedidoRequestApiModel>())
            .Select(item => new ItemCompraServiceModel { IdJogo = item.IdJogo, Quantidade = item.Quantidade })
            .ToList();
    }

    public static PedidoResponseApiModel ParaResponse(this PedidoDetalheServiceModel detalhe)
    {
        return new PedidoResponseApiModel
        {
            Id = detalhe.Pedido.Id,
            DataPedido = detalhe.Pedido.DataPedido,
            ValorTotal = detalhe.Pedido.ValorTotal,
            IdSituacaoPedido = detalhe.Pedido.IdSituacaoPedido,
            DescricaoSituacao = detalhe.DescricaoSituacao,
            Itens = detalhe.Itens
                .Select(item => new ItemPedidoResponseApiModel
                {
                    IdJogo = item.ItemPedido.IdJogo,
                    NomeJogo = item.NomeJogo,
                    Quantidade = item.ItemPedido.Quantidade,
                    ValorUnitario = item.ItemPedido.ValorUnitario,
                    Subtotal = item.ItemPedido.ValorUnitario * item.ItemPedido.Quantidade
                })
                .ToList()
        };
    }
}
