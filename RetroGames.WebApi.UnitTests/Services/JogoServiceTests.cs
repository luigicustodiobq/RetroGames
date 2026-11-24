using Moq;
using RetroGames.Model.Model;
using RetroGames.Repository;
using RetroGames.Services;
using RetroGames.Services.Erros;
using RetroGames.Services.ServiceModels;

namespace RetroGames.WebApi.UnitTests.Services;

/// <summary>
/// Regras de negócio de JogoService: relacionamentos obrigatórios (RN05), exclusão lógica (RN03)
/// e composição dos dados com LINQ (uma tabela por consulta no Repository).
/// </summary>
public class JogoServiceTests
{
    private readonly Mock<IJogoRepository> jogoRepository = new();
    private readonly Mock<IJogoGeneroRepository> jogoGeneroRepository = new();
    private readonly Mock<IPlataformaRepository> plataformaRepository = new();
    private readonly Mock<IGeneroRepository> generoRepository = new();
    private readonly Mock<ICondicaoJogoRepository> condicaoJogoRepository = new();
    private readonly Mock<IRepositorySet> repositorio = new();
    private readonly Mock<IUsuarioAtualService> usuarioAtual = new();
    private readonly JogoService service;

    public JogoServiceTests()
    {
        repositorio.Setup(r => r.Jogo).Returns(jogoRepository.Object);
        repositorio.Setup(r => r.JogoGenero).Returns(jogoGeneroRepository.Object);
        repositorio.Setup(r => r.Plataforma).Returns(plataformaRepository.Object);
        repositorio.Setup(r => r.Genero).Returns(generoRepository.Object);
        repositorio.Setup(r => r.CondicaoJogo).Returns(condicaoJogoRepository.Object);

        // Dados de domínio usados nas validações e na montagem dos detalhes.
        plataformaRepository.Setup(r => r.BuscarPorIdAsync(1))
            .ReturnsAsync(new Plataforma { Id = 1, Nome = "Super Nintendo", Fabricante = "Nintendo", AnoLancamento = 1990 });
        plataformaRepository.Setup(r => r.ListarAsync()).ReturnsAsync(new List<Plataforma>
        {
            new() { Id = 1, Nome = "Super Nintendo", Fabricante = "Nintendo", AnoLancamento = 1990 },
            new() { Id = 2, Nome = "Mega Drive", Fabricante = "Sega", AnoLancamento = 1988 }
        });
        condicaoJogoRepository.Setup(r => r.ListarAsync()).ReturnsAsync(new List<CondicaoJogo>
        {
            new() { Id = 1, Descricao = "Novo" },
            new() { Id = 2, Descricao = "Usado" }
        });
        generoRepository.Setup(r => r.ListarAsync()).ReturnsAsync(new List<Genero>
        {
            new() { Id = 1, Nome = "Plataforma" },
            new() { Id = 2, Nome = "RPG" }
        });

        // Padrão: sem vínculos de gênero (testes específicos sobrescrevem).
        jogoGeneroRepository
            .Setup(r => r.ListarPorJogosAsync(It.IsAny<IReadOnlyCollection<int>>()))
            .ReturnsAsync(new List<JogoGenero>());

        service = new JogoService(repositorio.Object, usuarioAtual.Object);
    }

    private static Jogo NovoJogo(int idPlataforma = 1, int idCondicao = 1) => new()
    {
        Nome = "Super Mario World",
        AnoLancamento = 1990,
        Preco = 199.90m,
        Estoque = 5,
        IdPlataforma = idPlataforma,
        IdCondicaoJogo = idCondicao
    };

    [Fact]
    public async Task Inserir_PlataformaInexistente_LancaPlataformaNaoEncontradaENaoGrava()
    {
        // Arrange
        plataformaRepository.Setup(r => r.BuscarPorIdAsync(99)).ReturnsAsync((Plataforma?)null);

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.InserirAsync(NovoJogo(idPlataforma: 99), new List<int> { 1 }));

        // Assert
        Assert.Equal(BusinessErrors.PLATAFORMA_NAO_ENCONTRADA, excecao.Erro);
        jogoRepository.Verify(r => r.InserirAsync(It.IsAny<Jogo>()), Times.Never);
    }

    [Fact]
    public async Task Inserir_CondicaoInexistente_LancaCondicaoNaoEncontradaENaoGrava()
    {
        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.InserirAsync(NovoJogo(idCondicao: 99), new List<int> { 1 }));

        // Assert
        Assert.Equal(BusinessErrors.CONDICAO_NAO_ENCONTRADA, excecao.Erro);
        jogoRepository.Verify(r => r.InserirAsync(It.IsAny<Jogo>()), Times.Never);
    }

    [Fact]
    public async Task Inserir_GeneroInexistente_LancaGeneroNaoEncontradoENaoGrava()
    {
        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.InserirAsync(NovoJogo(), new List<int> { 1, 99 }));

        // Assert
        Assert.Equal(BusinessErrors.GENERO_NAO_ENCONTRADO, excecao.Erro);
        jogoRepository.Verify(r => r.InserirAsync(It.IsAny<Jogo>()), Times.Never);
        jogoGeneroRepository.Verify(
            r => r.InserirAsync(It.IsAny<int>(), It.IsAny<IReadOnlyCollection<int>>()), Times.Never);
    }

    [Fact]
    public async Task Listar_FiltrandoPorGenero_EnviaAoRepositorySoOsIdsDosJogosDoGenero()
    {
        // Arrange: jogos 10 e 30 são do gênero 2; o jogo 20 é de outro gênero.
        jogoGeneroRepository.Setup(r => r.ListarTodosAsync()).ReturnsAsync(new List<JogoGenero>
        {
            new() { IdJogo = 10, IdGenero = 2 },
            new() { IdJogo = 20, IdGenero = 1 },
            new() { IdJogo = 30, IdGenero = 2 }
        });
        jogoRepository
            .Setup(r => r.ListarAsync(It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<IReadOnlyCollection<int>?>(),
                It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((new List<Jogo>(), 0));

        // Act
        await service.ListarAsync(new FiltroJogoServiceModel { IdGenero = 2 });

        // Assert
        jogoRepository.Verify(r => r.ListarAsync(
            null, null,
            It.Is<IReadOnlyCollection<int>?>(ids => ids != null && ids.OrderBy(id => id).SequenceEqual(new[] { 10, 30 })),
            false, 1, 12), Times.Once);
    }

    [Fact]
    public async Task Listar_SemGenero_NaoConsultaVinculosENaoRestringeIds()
    {
        // Arrange
        jogoRepository
            .Setup(r => r.ListarAsync(It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<IReadOnlyCollection<int>?>(),
                It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((new List<Jogo>(), 0));

        // Act
        await service.ListarAsync(new FiltroJogoServiceModel { Busca = "Mario" });

        // Assert
        jogoGeneroRepository.Verify(r => r.ListarTodosAsync(), Times.Never);
        jogoRepository.Verify(r => r.ListarAsync("Mario", null, null, false, 1, 12), Times.Once);
    }

    [Fact]
    public async Task Listar_PaginaComJogos_MontaPlataformaECondicaoComLinq()
    {
        // Arrange
        var jogos = new List<Jogo>
        {
            new() { Id = 10, Nome = "Super Mario World", IdPlataforma = 1, IdCondicaoJogo = 2, Ativo = true },
            new() { Id = 20, Nome = "Sonic", IdPlataforma = 2, IdCondicaoJogo = 1, Ativo = true }
        };
        jogoRepository
            .Setup(r => r.ListarAsync(null, null, null, false, 1, 12))
            .ReturnsAsync((jogos, 2));
        jogoGeneroRepository
            .Setup(r => r.ListarPorJogosAsync(It.IsAny<IReadOnlyCollection<int>>()))
            .ReturnsAsync(new List<JogoGenero> { new() { IdJogo = 10, IdGenero = 1 } });

        // Act
        var pagina = await service.ListarAsync(new FiltroJogoServiceModel());

        // Assert
        Assert.Equal(2, pagina.TotalItens);
        var mario = pagina.Itens.Single(item => item.Jogo.Id == 10);
        Assert.Equal("Super Nintendo", mario.NomePlataforma);
        Assert.Equal("Usado", mario.DescricaoCondicao);
        Assert.Equal("Plataforma", Assert.Single(mario.Generos).Nome);

        var sonic = pagina.Itens.Single(item => item.Jogo.Id == 20);
        Assert.Equal("Mega Drive", sonic.NomePlataforma);
        Assert.Equal("Novo", sonic.DescricaoCondicao);
        Assert.Empty(sonic.Generos);
    }

    [Fact]
    public async Task BuscarPorId_IdInexistente_LancaJogoNaoEncontrado()
    {
        // Arrange
        jogoRepository.Setup(r => r.BuscarPorIdAsync(999)).ReturnsAsync((Jogo?)null);

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(() => service.BuscarPorIdAsync(999));

        // Assert
        Assert.Equal(BusinessErrors.JOGO_NAO_ENCONTRADO, excecao.Erro);
    }

    [Fact]
    public async Task Desativar_IdInexistente_LancaJogoNaoEncontradoENaoDesativa()
    {
        // Arrange
        jogoRepository.Setup(r => r.BuscarPorIdAsync(999)).ReturnsAsync((Jogo?)null);

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(() => service.DesativarAsync(999));

        // Assert
        Assert.Equal(BusinessErrors.JOGO_NAO_ENCONTRADO, excecao.Erro);
        jogoRepository.Verify(r => r.DesativarAsync(It.IsAny<int>()), Times.Never);
    }

    // ---------------------------------------------------------------------
    // RN04: jogo inativo só é visível para administrador (IUsuarioAtualService mockado)
    // ---------------------------------------------------------------------

    private void ConfigurarListagemVazia()
    {
        jogoRepository
            .Setup(r => r.ListarAsync(It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<IReadOnlyCollection<int>?>(),
                It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((new List<Jogo>(), 0));
    }

    [Fact]
    public async Task BuscarPorId_JogoInativoParaVisitante_LancaJogoNaoEncontrado()
    {
        // Arrange: visitante (não autenticado) e jogo desativado logicamente.
        usuarioAtual.Setup(u => u.EstaAutenticado).Returns(false);
        usuarioAtual.Setup(u => u.EhAdministrador).Returns(false);
        jogoRepository.Setup(r => r.BuscarPorIdAsync(9))
            .ReturnsAsync(new Jogo { Id = 9, Nome = "Mortal Kombat", IdPlataforma = 1, IdCondicaoJogo = 2, Ativo = false });

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(() => service.BuscarPorIdAsync(9));

        // Assert: para quem não é administrador, jogo inativo é como se não existisse.
        Assert.Equal(BusinessErrors.JOGO_NAO_ENCONTRADO, excecao.Erro);
    }

    [Fact]
    public async Task BuscarPorId_JogoInativoParaAdministrador_DevolveODetalhe()
    {
        // Arrange
        usuarioAtual.Setup(u => u.EstaAutenticado).Returns(true);
        usuarioAtual.Setup(u => u.EhAdministrador).Returns(true);
        jogoRepository.Setup(r => r.BuscarPorIdAsync(9))
            .ReturnsAsync(new Jogo { Id = 9, Nome = "Mortal Kombat", IdPlataforma = 1, IdCondicaoJogo = 2, Ativo = false });

        // Act
        var detalhe = await service.BuscarPorIdAsync(9);

        // Assert
        Assert.False(detalhe.Jogo.Ativo);
        Assert.Equal("Super Nintendo", detalhe.NomePlataforma);
    }

    [Fact]
    public async Task Listar_VisitantePedindoInativos_RepositoryRecebeIncluirInativosFalse()
    {
        // Arrange
        usuarioAtual.Setup(u => u.EhAdministrador).Returns(false);
        ConfigurarListagemVazia();

        // Act
        await service.ListarAsync(new FiltroJogoServiceModel { IncluirInativos = true });

        // Assert: o pedido de inativos é ignorado para quem não é administrador.
        jogoRepository.Verify(r => r.ListarAsync(null, null, null, false, 1, 12), Times.Once);
        jogoRepository.Verify(r => r.ListarAsync(It.IsAny<string?>(), It.IsAny<int?>(),
            It.IsAny<IReadOnlyCollection<int>?>(), true, It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Listar_AdministradorComIncluirInativos_RepositoryRecebeIncluirInativosTrue()
    {
        // Arrange
        usuarioAtual.Setup(u => u.EstaAutenticado).Returns(true);
        usuarioAtual.Setup(u => u.EhAdministrador).Returns(true);
        ConfigurarListagemVazia();

        // Act
        await service.ListarAsync(new FiltroJogoServiceModel { IncluirInativos = true });

        // Assert
        jogoRepository.Verify(r => r.ListarAsync(null, null, null, true, 1, 12), Times.Once);
    }

    [Fact]
    public async Task Alterar_ComAtivoTrue_ReativaJogoDesativado()
    {
        // Arrange: o jogo existe inativo; o administrador envia Ativo = true no PUT.
        usuarioAtual.Setup(u => u.EhAdministrador).Returns(true);
        jogoRepository.SetupSequence(r => r.BuscarPorIdAsync(9))
            .ReturnsAsync(new Jogo { Id = 9, Nome = "Mortal Kombat", IdPlataforma = 1, IdCondicaoJogo = 2, Ativo = false })
            .ReturnsAsync(new Jogo { Id = 9, Nome = "Mortal Kombat", IdPlataforma = 1, IdCondicaoJogo = 2, Ativo = true });
        var jogo = NovoJogo();
        jogo.Id = 9;
        jogo.Ativo = true;

        // Act
        var detalhe = await service.AlterarAsync(jogo, new List<int> { 1 });

        // Assert
        jogoRepository.Verify(r => r.AlterarAsync(It.Is<Jogo>(j => j.Id == 9 && j.Ativo)), Times.Once);
        Assert.True(detalhe.Jogo.Ativo);
    }
}
