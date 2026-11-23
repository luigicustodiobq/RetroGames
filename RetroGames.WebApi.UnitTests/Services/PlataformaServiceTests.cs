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
}
