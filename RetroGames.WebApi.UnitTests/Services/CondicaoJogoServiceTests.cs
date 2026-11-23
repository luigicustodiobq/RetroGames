using Moq;
using RetroGames.Model.Constantes;
using RetroGames.Model.Model;
using RetroGames.Repository;
using RetroGames.Services;

namespace RetroGames.WebApi.UnitTests.Services;

/// <summary>Listagem das condições de jogo (tabela de domínio com chaves fixas).</summary>
public class CondicaoJogoServiceTests
{
    private readonly Mock<ICondicaoJogoRepository> condicaoJogoRepository = new();
    private readonly Mock<IRepositorySet> repositorio = new();
    private readonly CondicaoJogoService service;

    public CondicaoJogoServiceTests()
    {
        repositorio.Setup(r => r.CondicaoJogo).Returns(condicaoJogoRepository.Object);
        service = new CondicaoJogoService(repositorio.Object);
    }

    [Fact]
    public async Task Listar_DevolveAsCondicoesComAsChavesFixas()
    {
        // Arrange
        condicaoJogoRepository.Setup(r => r.ListarAsync()).ReturnsAsync(new List<CondicaoJogo>
        {
            new() { Id = CondicaoJogoIds.Novo, Descricao = "Novo" },
            new() { Id = CondicaoJogoIds.Usado, Descricao = "Usado" },
            new() { Id = CondicaoJogoIds.Recondicionado, Descricao = "Recondicionado" }
        });

        // Act
        var resultado = await service.ListarAsync();

        // Assert
        Assert.Equal(3, resultado.Count);
        Assert.Equal(new[] { 1, 2, 3 }, resultado.Select(c => c.Id));
        Assert.Equal("Usado", resultado.Single(c => c.Id == CondicaoJogoIds.Usado).Descricao);
        condicaoJogoRepository.Verify(r => r.ListarAsync(), Times.Once);
    }
}
