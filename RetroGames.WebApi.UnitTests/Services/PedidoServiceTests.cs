using Moq;
using RetroGames.Model.Constantes;
using RetroGames.Model.Model;
using RetroGames.Repository;
using RetroGames.Services;
using RetroGames.Services.Erros;
using RetroGames.Services.ServiceModels;

namespace RetroGames.WebApi.UnitTests.Services;

/// <summary>Módulo de Compras: finalização da compra (C04) e pedidos do usuário logado.</summary>
public class PedidoServiceTests
{
    private const int IdUsuarioLogado = 5;

    private readonly Mock<IJogoRepository> jogoRepository = new();
    private readonly Mock<IPedidoRepository> pedidoRepository = new();
    private readonly Mock<IItemPedidoRepository> itemPedidoRepository = new();
    private readonly Mock<ISituacaoPedidoRepository> situacaoPedidoRepository = new();
    private readonly Mock<IRepositorySet> repositorio = new();
    private readonly Mock<IUsuarioAtualService> usuarioAtual = new();
    private readonly PedidoService service;

    public PedidoServiceTests()
    {
        repositorio.Setup(r => r.Jogo).Returns(jogoRepository.Object);
        repositorio.Setup(r => r.Pedido).Returns(pedidoRepository.Object);
        repositorio.Setup(r => r.ItemPedido).Returns(itemPedidoRepository.Object);
        repositorio.Setup(r => r.SituacaoPedido).Returns(situacaoPedidoRepository.Object);

        usuarioAtual.Setup(u => u.EstaAutenticado).Returns(true);
        usuarioAtual.Setup(u => u.IdUsuario).Returns(IdUsuarioLogado);

        situacaoPedidoRepository.Setup(r => r.ListarAsync()).ReturnsAsync(new List<SituacaoPedido>
        {
            new() { Id = SituacaoPedidoIds.Pendente, Descricao = "Pendente" },
            new() { Id = SituacaoPedidoIds.Pago, Descricao = "Pago" },
            new() { Id = SituacaoPedidoIds.Enviado, Descricao = "Enviado" },
            new() { Id = SituacaoPedidoIds.Cancelado, Descricao = "Cancelado" }
        });

        service = new PedidoService(repositorio.Object, usuarioAtual.Object);
    }

    private void CadastrarJogo(int id, string nome, decimal preco, int estoque, bool ativo = true)
    {
        jogoRepository.Setup(r => r.BuscarPorIdAsync(id)).ReturnsAsync(new Jogo
        {
            Id = id, Nome = nome, Preco = preco, Estoque = estoque, Ativo = ativo
        });
    }

    private void VerificarQueNadaFoiGravado()
    {
        pedidoRepository.Verify(r => r.InserirAsync(It.IsAny<Pedido>()), Times.Never);
        itemPedidoRepository.Verify(r => r.InserirAsync(It.IsAny<ItemPedido>()), Times.Never);
        jogoRepository.Verify(r => r.AtualizarEstoqueAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task FinalizarCompra_ItensValidos_CalculaTotalPeloPrecoDoBancoEBaixaEstoque()
    {
        // Arrange
        CadastrarJogo(1, "Super Mario World", 199.90m, estoque: 5);
        CadastrarJogo(2, "Sonic", 89.50m, estoque: 3);

        Pedido? pedidoGravado = null;
        pedidoRepository
            .Setup(r => r.InserirAsync(It.IsAny<Pedido>()))
            .Callback<Pedido>(pedido => pedidoGravado = pedido)
            .ReturnsAsync(100);
        var itensGravados = new List<ItemPedido>();
        itemPedidoRepository
            .Setup(r => r.InserirAsync(It.IsAny<ItemPedido>()))
            .Callback<ItemPedido>(item => itensGravados.Add(item))
            .ReturnsAsync(1);
        jogoRepository.Setup(r => r.AtualizarEstoqueAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);

        var itens = new List<ItemCompraServiceModel>
        {
            new() { IdJogo = 1, Quantidade = 2 },
            new() { IdJogo = 2, Quantidade = 1 }
        };

        // Act
        var resultado = await service.FinalizarCompraAsync(itens);

        // Assert: 2 x 199,90 + 1 x 89,50 = 489,30, calculado com os preços lidos do banco.
        Assert.NotNull(pedidoGravado);
        Assert.Equal(489.30m, pedidoGravado!.ValorTotal);
        Assert.Equal(IdUsuarioLogado, pedidoGravado.IdUsuario);
        Assert.Equal(SituacaoPedidoIds.Pendente, pedidoGravado.IdSituacaoPedido);
        Assert.Equal(DateTimeKind.Utc, pedidoGravado.DataPedido.Kind);

        Assert.Equal(2, itensGravados.Count);
        Assert.All(itensGravados, item => Assert.Equal(100, item.IdPedido));
        Assert.Equal(199.90m, itensGravados.Single(item => item.IdJogo == 1).ValorUnitario);

        jogoRepository.Verify(r => r.AtualizarEstoqueAsync(1, 2), Times.Once);
        jogoRepository.Verify(r => r.AtualizarEstoqueAsync(2, 1), Times.Once);

        Assert.Equal(100, resultado.Pedido.Id);
        Assert.Equal("Pendente", resultado.DescricaoSituacao);
        Assert.Equal("Super Mario World", resultado.Itens.Single(item => item.ItemPedido.IdJogo == 1).NomeJogo);
    }

    [Fact]
    public async Task FinalizarCompra_MesmoJogoRepetido_ConsolidaNumUnicoItem()
    {
        // Arrange
        CadastrarJogo(1, "Super Mario World", 100m, estoque: 5);
        pedidoRepository.Setup(r => r.InserirAsync(It.IsAny<Pedido>())).ReturnsAsync(1);
        jogoRepository.Setup(r => r.AtualizarEstoqueAsync(1, 3)).ReturnsAsync(true);

        // Act
        await service.FinalizarCompraAsync(new List<ItemCompraServiceModel>
        {
            new() { IdJogo = 1, Quantidade = 1 },
            new() { IdJogo = 1, Quantidade = 2 }
        });

        // Assert
        itemPedidoRepository.Verify(r => r.InserirAsync(It.Is<ItemPedido>(i => i.IdJogo == 1 && i.Quantidade == 3)), Times.Once);
        jogoRepository.Verify(r => r.AtualizarEstoqueAsync(1, 3), Times.Once);
    }

    [Fact]
    public async Task FinalizarCompra_JogoInativo_LancaJogoIndisponivelSemGravar()
    {
        // Arrange
        CadastrarJogo(1, "Mortal Kombat", 120m, estoque: 4, ativo: false);

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.FinalizarCompraAsync(new List<ItemCompraServiceModel> { new() { IdJogo = 1, Quantidade = 1 } }));

        // Assert
        Assert.Equal(BusinessErrors.JOGO_INDISPONIVEL, excecao.Erro);
        Assert.Equal(110, excecao.Erro.Code);
        VerificarQueNadaFoiGravado();
    }

    [Fact]
    public async Task FinalizarCompra_JogoInexistente_LancaJogoIndisponivelSemGravar()
    {
        // Arrange
        jogoRepository.Setup(r => r.BuscarPorIdAsync(999)).ReturnsAsync((Jogo?)null);

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.FinalizarCompraAsync(new List<ItemCompraServiceModel> { new() { IdJogo = 999, Quantidade = 1 } }));

        // Assert
        Assert.Equal(BusinessErrors.JOGO_INDISPONIVEL, excecao.Erro);
        VerificarQueNadaFoiGravado();
    }

    [Fact]
    public async Task FinalizarCompra_QuantidadeAcimaDoEstoque_LancaEstoqueInsuficienteSemGravar()
    {
        // Arrange
        CadastrarJogo(1, "Super Mario World", 199.90m, estoque: 2);

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.FinalizarCompraAsync(new List<ItemCompraServiceModel> { new() { IdJogo = 1, Quantidade = 3 } }));

        // Assert
        Assert.Equal(BusinessErrors.ESTOQUE_INSUFICIENTE, excecao.Erro);
        Assert.Equal(111, excecao.Erro.Code);
        VerificarQueNadaFoiGravado();
    }

    [Fact]
    public async Task ListarMeusPedidos_ConsultaSoOUsuarioLogadoEMontaSituacaoEItensComLinq()
    {
        // Arrange: o Repository devolve fora de ordem; o Service ordena do mais recente para o mais antigo.
        var antigo = new Pedido
        {
            Id = 1, IdUsuario = IdUsuarioLogado, DataPedido = new DateTime(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc),
            ValorTotal = 199.90m, IdSituacaoPedido = SituacaoPedidoIds.Enviado
        };
        var recente = new Pedido
        {
            Id = 2, IdUsuario = IdUsuarioLogado, DataPedido = new DateTime(2026, 11, 20, 15, 30, 0, DateTimeKind.Utc),
            ValorTotal = 179.00m, IdSituacaoPedido = SituacaoPedidoIds.Pendente
        };
        pedidoRepository.Setup(r => r.ListarPorUsuarioAsync(IdUsuarioLogado)).ReturnsAsync(new List<Pedido> { antigo, recente });
        itemPedidoRepository
            .Setup(r => r.ListarPorPedidosAsync(It.IsAny<IReadOnlyCollection<int>>()))
            .ReturnsAsync(new List<ItemPedido>
            {
                new() { Id = 10, IdPedido = 1, IdJogo = 7, Quantidade = 1, ValorUnitario = 199.90m },
                new() { Id = 11, IdPedido = 2, IdJogo = 8, Quantidade = 2, ValorUnitario = 89.50m }
            });
        jogoRepository
            .Setup(r => r.ListarAsync(null, null, It.IsAny<IReadOnlyCollection<int>?>(), true, 1, It.IsAny<int>()))
            .ReturnsAsync((new List<Jogo>
            {
                new() { Id = 7, Nome = "Chrono Trigger" },
                new() { Id = 8, Nome = "Sonic" }
            }, 2));

        // Act
        var pedidos = await service.ListarMeusPedidosAsync();

        // Assert
        pedidoRepository.Verify(r => r.ListarPorUsuarioAsync(IdUsuarioLogado), Times.Once);
        Assert.Equal(new[] { 2, 1 }, pedidos.Select(p => p.Pedido.Id));

        Assert.Equal("Pendente", pedidos[0].DescricaoSituacao);
        var itemRecente = Assert.Single(pedidos[0].Itens);
        Assert.Equal("Sonic", itemRecente.NomeJogo);
        Assert.Equal(2, itemRecente.ItemPedido.Quantidade);
        Assert.Equal(89.50m, itemRecente.ItemPedido.ValorUnitario);

        Assert.Equal("Enviado", pedidos[1].DescricaoSituacao);
        Assert.Equal("Chrono Trigger", Assert.Single(pedidos[1].Itens).NomeJogo);
    }

    [Fact]
    public async Task ListarMeusPedidos_SemPedidos_DevolveListaVaziaSemConsultarItens()
    {
        // Arrange
        pedidoRepository.Setup(r => r.ListarPorUsuarioAsync(IdUsuarioLogado)).ReturnsAsync(new List<Pedido>());

        // Act
        var pedidos = await service.ListarMeusPedidosAsync();

        // Assert
        Assert.Empty(pedidos);
        itemPedidoRepository.Verify(r => r.ListarPorPedidosAsync(It.IsAny<IReadOnlyCollection<int>>()), Times.Never);
    }

    [Fact]
    public async Task BuscarMeuPedido_IdInexistente_LancaPedidoNaoEncontrado()
    {
        // Arrange
        pedidoRepository.Setup(r => r.BuscarPorIdAsync(77)).ReturnsAsync((Pedido?)null);

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(() => service.BuscarMeuPedidoAsync(77));

        // Assert
        Assert.Equal(BusinessErrors.PEDIDO_NAO_ENCONTRADO, excecao.Erro);
    }

    [Fact]
    public async Task BuscarMeuPedido_PedidoDeOutroUsuario_LancaPedidoDeOutroUsuario()
    {
        // Arrange
        pedidoRepository.Setup(r => r.BuscarPorIdAsync(8)).ReturnsAsync(new Pedido { Id = 8, IdUsuario = 99 });

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(() => service.BuscarMeuPedidoAsync(8));

        // Assert
        Assert.Equal(BusinessErrors.PEDIDO_DE_OUTRO_USUARIO, excecao.Erro);
        itemPedidoRepository.Verify(r => r.ListarPorPedidosAsync(It.IsAny<IReadOnlyCollection<int>>()), Times.Never);
    }
}
