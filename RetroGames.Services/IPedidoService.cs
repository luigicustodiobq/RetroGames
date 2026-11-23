using RetroGames.Services.ServiceModels;

namespace RetroGames.Services;

/// <summary>Regras de negócio do Módulo de Compras.</summary>
public interface IPedidoService
{
    /// <summary>
    /// C04: grava o pedido do usuário logado, com os preços do banco, e baixa o estoque,
    /// tudo numa transação. Erros 110 (jogo indisponível) e 111 (estoque insuficiente).
    /// </summary>
    Task<PedidoDetalheServiceModel> FinalizarCompraAsync(IReadOnlyCollection<ItemCompraServiceModel> itens);

    /// <summary>Um pedido do usuário logado; erros 109 (não existe) e 113 (é de outro usuário).</summary>
    Task<PedidoDetalheServiceModel> BuscarMeuPedidoAsync(int id);
}
