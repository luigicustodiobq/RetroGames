using System.Transactions;
using RetroGames.Model.Constantes;
using RetroGames.Model.Model;
using RetroGames.Repository;
using RetroGames.Services.Erros;
using RetroGames.Services.ServiceModels;

namespace RetroGames.Services;

/// <summary>
/// Regras de negócio do Módulo de Compras. O usuário vem sempre da sessão (IUsuarioAtualService),
/// nunca do corpo da requisição, e o preço vem sempre do banco, nunca do front-end.
/// </summary>
public class PedidoService : IPedidoService
{
    private readonly IRepositorySet repositorio;
    private readonly IUsuarioAtualService usuarioAtual;

    public PedidoService(IRepositorySet repositorio, IUsuarioAtualService usuarioAtual)
    {
        this.repositorio = repositorio;
        this.usuarioAtual = usuarioAtual;
    }

    public async Task<PedidoDetalheServiceModel> FinalizarCompraAsync(IReadOnlyCollection<ItemCompraServiceModel> itens)
    {
        var idUsuario = ObterIdUsuarioLogado();

        // O mesmo jogo enviado duas vezes vira um único item (tb_item_pedido tem UNIQUE (co_pedido, co_jogo)).
        var itensConsolidados = itens
            .GroupBy(item => item.IdJogo)
            .Select(grupo => new ItemCompraServiceModel { IdJogo = grupo.Key, Quantidade = grupo.Sum(item => item.Quantidade) })
            .ToList();

        // Confere cada jogo no banco antes de abrir a transação.
        var jogosComprados = new List<(Jogo Jogo, int Quantidade)>();
        foreach (var item in itensConsolidados)
        {
            var jogo = await repositorio.Jogo.BuscarPorIdAsync(item.IdJogo);
            if (jogo is null || !jogo.Ativo)
            {
                throw new BusinessRuleException(BusinessErrors.JOGO_INDISPONIVEL);
            }
            if (jogo.Estoque < item.Quantidade)
            {
                throw new BusinessRuleException(BusinessErrors.ESTOQUE_INSUFICIENTE);
            }
            jogosComprados.Add((jogo, item.Quantidade));
        }

        var pedido = new Pedido
        {
            DataPedido = DateTime.UtcNow,
            ValorTotal = jogosComprados.Sum(comprado => comprado.Jogo.Preco * comprado.Quantidade),
            IdUsuario = idUsuario,
            IdSituacaoPedido = SituacaoPedidoIds.Pendente
        };
        var itensGravados = new List<ItemPedido>();

        // Pedido, itens e baixa de estoque: ou tudo é gravado, ou nada é (sem Complete, o scope desfaz).
        using (var transacao = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            pedido.Id = await repositorio.Pedido.InserirAsync(pedido);

            foreach (var (jogo, quantidade) in jogosComprados)
            {
                var itemPedido = new ItemPedido
                {
                    IdPedido = pedido.Id,
                    IdJogo = jogo.Id,
                    Quantidade = quantidade,
                    ValorUnitario = jogo.Preco
                };
                itemPedido.Id = await repositorio.ItemPedido.InserirAsync(itemPedido);
                itensGravados.Add(itemPedido);

                // false: outra compra levou as unidades entre a conferência e a baixa.
                if (!await repositorio.Jogo.AtualizarEstoqueAsync(jogo.Id, quantidade))
                {
                    throw new BusinessRuleException(BusinessErrors.ESTOQUE_INSUFICIENTE);
                }
            }

            transacao.Complete();
        }

        var nomesJogos = jogosComprados.ToDictionary(comprado => comprado.Jogo.Id, comprado => comprado.Jogo.Nome);
        var situacoes = await ListarSituacoesAsync();
        return Montar(pedido, itensGravados, nomesJogos, situacoes);
    }

    public async Task<PedidoDetalheServiceModel> BuscarMeuPedidoAsync(int id)
    {
        var idUsuario = ObterIdUsuarioLogado();

        var pedido = await repositorio.Pedido.BuscarPorIdAsync(id)
            ?? throw new BusinessRuleException(BusinessErrors.PEDIDO_NAO_ENCONTRADO);

        if (pedido.IdUsuario != idUsuario)
        {
            throw new BusinessRuleException(BusinessErrors.PEDIDO_DE_OUTRO_USUARIO);
        }

        var detalhes = await MontarDetalhesAsync(new List<Pedido> { pedido });
        return detalhes[0];
    }

    /// <summary>Id do usuário da sessão. As rotas de pedido exigem login ([Authorize]).</summary>
    private int ObterIdUsuarioLogado()
    {
        return usuarioAtual.IdUsuario
            ?? throw new InvalidOperationException("Operação de pedido sem usuário autenticado.");
    }

    private async Task<Dictionary<int, string>> ListarSituacoesAsync()
    {
        return (await repositorio.SituacaoPedido.ListarAsync()).ToDictionary(s => s.Id, s => s.Descricao);
    }

    /// <summary>
    /// Monta o detalhe de uma lista de pedidos com 3 consultas, qualquer que seja o tamanho da lista:
    /// situações, itens dos pedidos e jogos dos itens.
    /// </summary>
    private async Task<List<PedidoDetalheServiceModel>> MontarDetalhesAsync(List<Pedido> pedidos)
    {
        if (pedidos.Count == 0)
        {
            return new List<PedidoDetalheServiceModel>();
        }

        var situacoes = await ListarSituacoesAsync();
        var itens = await repositorio.ItemPedido.ListarPorPedidosAsync(pedidos.Select(p => p.Id).ToList());

        var idsJogos = itens.Select(item => item.IdJogo).Distinct().ToList();
        var nomesJogos = new Dictionary<int, string>();
        if (idsJogos.Count > 0)
        {
            // Uma consulta para todos os jogos dos itens (inclusive os que foram desativados depois).
            var (jogos, _) = await repositorio.Jogo.ListarAsync(null, null, idsJogos, true, 1, idsJogos.Count);
            nomesJogos = jogos.ToDictionary(jogo => jogo.Id, jogo => jogo.Nome);
        }

        var itensPorPedido = itens.ToLookup(item => item.IdPedido);
        return pedidos
            .Select(pedido => Montar(pedido, itensPorPedido[pedido.Id], nomesJogos, situacoes))
            .ToList();
    }

    private static PedidoDetalheServiceModel Montar(
        Pedido pedido,
        IEnumerable<ItemPedido> itens,
        IReadOnlyDictionary<int, string> nomesJogos,
        IReadOnlyDictionary<int, string> situacoes)
    {
        return new PedidoDetalheServiceModel
        {
            Pedido = pedido,
            DescricaoSituacao = situacoes.TryGetValue(pedido.IdSituacaoPedido, out var situacao) ? situacao : string.Empty,
            Itens = itens
                .Select(item => new ItemPedidoDetalheServiceModel
                {
                    ItemPedido = item,
                    NomeJogo = nomesJogos.TryGetValue(item.IdJogo, out var nome) ? nome : string.Empty
                })
                .ToList()
        };
    }
}
