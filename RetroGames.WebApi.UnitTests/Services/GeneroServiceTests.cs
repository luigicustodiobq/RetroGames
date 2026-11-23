using Moq;
using RetroGames.Model.Model;
using RetroGames.Repository;
using RetroGames.Services;

namespace RetroGames.WebApi.UnitTests.Services;

/// <summary>Listagem de gêneros: o Service devolve o que o Repository leu, sem alterar.</summary>
public class GeneroServiceTests
{
    private readonly Mock<IGeneroRepository> generoRepository = new();
    private readonly Mock<IRepositorySet> repositorio = new();
    private readonly GeneroService service;

    public GeneroServiceTests()
    {
        repositorio.Setup(r => r.Genero).Returns(generoRepository.Object);
        service = new GeneroService(repositorio.Object);
    }

    [Fact]
    public async Task Listar_DevolveOsGenerosDoRepository()
    {
        // Arrange
        var generos = new List<Genero>
        {
            new() { Id = 1, Nome = "Ação" },
            new() { Id = 2, Nome = "RPG" }
        };
        generoRepository.Setup(r => r.ListarAsync()).ReturnsAsync(generos);

        // Act
        var resultado = await service.ListarAsync();

        // Assert
        Assert.Equal(2, resultado.Count);
        Assert.Equal(new[] { "Ação", "RPG" }, resultado.Select(g => g.Nome));
        generoRepository.Verify(r => r.ListarAsync(), Times.Once);
    }

    [Fact]
    public async Task Listar_SemGeneros_DevolveListaVazia()
    {
        // Arrange
        generoRepository.Setup(r => r.ListarAsync()).ReturnsAsync(new List<Genero>());

        // Act
        var resultado = await service.ListarAsync();

        // Assert
        Assert.Empty(resultado);
    }
}
