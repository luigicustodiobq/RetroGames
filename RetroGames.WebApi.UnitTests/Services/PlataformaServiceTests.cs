using Moq;
using RetroGames.Model.Model;
using RetroGames.Repository;
using RetroGames.Services;
using RetroGames.Services.Erros;

namespace RetroGames.WebApi.UnitTests.Services;

/// <summary>
/// Regras de negócio de PlataformaService. O banco não é usado: IPlataformaRepository
/// é um mock (Moq) devolvido pelo IRepositorySet, também mockado.
/// </summary>
public class PlataformaServiceTests
{
    private readonly Mock<IPlataformaRepository> plataformaRepository = new();
    private readonly Mock<IRepositorySet> repositorio = new();
    private readonly PlataformaService service;

    public PlataformaServiceTests()
    {
        repositorio.Setup(r => r.Plataforma).Returns(plataformaRepository.Object);
        service = new PlataformaService(repositorio.Object);
    }

    [Fact]
    public async Task BuscarPorId_IdInexistente_LancaNaoEncontrada()
    {
        // Arrange
        plataformaRepository.Setup(r => r.BuscarPorIdAsync(99)).ReturnsAsync((Plataforma?)null);

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(() => service.BuscarPorIdAsync(99));

        // Assert
        Assert.Equal(BusinessErrors.PLATAFORMA_NAO_ENCONTRADA, excecao.Erro);
    }

    [Fact]
    public async Task Inserir_NomeJaCadastrado_LancaNomeDuplicadoENaoGrava()
    {
        // Arrange
        var plataforma = new Plataforma { Nome = "Super Nintendo", Fabricante = "Nintendo", AnoLancamento = 1990 };
        plataformaRepository.Setup(r => r.ExisteComNomeAsync("Super Nintendo", null)).ReturnsAsync(true);

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(() => service.InserirAsync(plataforma));

        // Assert
        Assert.Equal(BusinessErrors.PLATAFORMA_NOME_DUPLICADO, excecao.Erro);
        plataformaRepository.Verify(r => r.InserirAsync(It.IsAny<Plataforma>()), Times.Never);
    }

    [Fact]
    public async Task Inserir_RepositoryLancaRegistroDuplicado_LancaNomeDuplicado()
    {
        // Arrange: a verificação passa, mas outra requisição gravou o mesmo nome antes do INSERT.
        var plataforma = new Plataforma { Nome = "Mega Drive", Fabricante = "Sega", AnoLancamento = 1988 };
        plataformaRepository.Setup(r => r.ExisteComNomeAsync("Mega Drive", null)).ReturnsAsync(false);
        plataformaRepository
            .Setup(r => r.InserirAsync(plataforma))
            .ThrowsAsync(new RegistroDuplicadoException("Duplicado", new Exception()));

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(() => service.InserirAsync(plataforma));

        // Assert
        Assert.Equal(BusinessErrors.PLATAFORMA_NOME_DUPLICADO, excecao.Erro);
    }

    [Fact]
    public async Task Inserir_NomeLivre_DevolvePlataformaComIdGerado()
    {
        // Arrange
        var plataforma = new Plataforma { Nome = "  Neo Geo  ", Fabricante = " SNK ", AnoLancamento = 1990 };
        plataformaRepository.Setup(r => r.ExisteComNomeAsync("Neo Geo", null)).ReturnsAsync(false);
        plataformaRepository.Setup(r => r.InserirAsync(plataforma)).ReturnsAsync(42);

        // Act
        var criada = await service.InserirAsync(plataforma);

        // Assert
        Assert.Equal(42, criada.Id);
        Assert.Equal("Neo Geo", criada.Nome);
        Assert.Equal("SNK", criada.Fabricante);
        plataformaRepository.Verify(r => r.InserirAsync(plataforma), Times.Once);
    }

    [Fact]
    public async Task Alterar_MantendoOProprioNome_AceitaEChamaAlterar()
    {
        // Arrange
        var existente = new Plataforma { Id = 1, Nome = "Super Nintendo", Fabricante = "Nintendo", AnoLancamento = 1990 };
        var alterada = new Plataforma { Id = 1, Nome = "Super Nintendo", Fabricante = "Nintendo", AnoLancamento = 1991 };
        plataformaRepository.Setup(r => r.BuscarPorIdAsync(1)).ReturnsAsync(existente);
        // Ignorando o próprio id (1), não há outra plataforma com o mesmo nome.
        plataformaRepository.Setup(r => r.ExisteComNomeAsync("Super Nintendo", 1)).ReturnsAsync(false);

        // Act
        var resultado = await service.AlterarAsync(alterada);

        // Assert
        Assert.Equal(1991, resultado.AnoLancamento);
        plataformaRepository.Verify(r => r.ExisteComNomeAsync("Super Nintendo", 1), Times.Once);
        plataformaRepository.Verify(r => r.AlterarAsync(alterada), Times.Once);
    }

    [Fact]
    public async Task Excluir_PlataformaComJogos_LancaPossuiJogosENaoExclui()
    {
        // Arrange
        plataformaRepository
            .Setup(r => r.BuscarPorIdAsync(1))
            .ReturnsAsync(new Plataforma { Id = 1, Nome = "Super Nintendo", Fabricante = "Nintendo", AnoLancamento = 1990 });
        plataformaRepository.Setup(r => r.PossuiJogosAsync(1)).ReturnsAsync(true);

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(() => service.ExcluirAsync(1));

        // Assert
        Assert.Equal(BusinessErrors.PLATAFORMA_POSSUI_JOGOS, excecao.Erro);
        plataformaRepository.Verify(r => r.ExcluirAsync(It.IsAny<int>()), Times.Never);
    }
}
