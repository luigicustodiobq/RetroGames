using Microsoft.AspNetCore.Identity;
using Moq;
using RetroGames.Model.Constantes;
using RetroGames.Model.Model;
using RetroGames.Repository;
using RetroGames.Services;
using RetroGames.Services.Erros;

namespace RetroGames.WebApi.UnitTests.Services;

/// <summary>C01: cadastro de cliente no AutenticacaoService.</summary>
public class AutenticacaoServiceTests
{
    private readonly Mock<IUsuarioRepository> usuarioRepository = new();
    private readonly Mock<IRepositorySet> repositorio = new();
    private readonly Mock<IPasswordHasher<Usuario>> passwordHasher = new();
    private readonly AutenticacaoService service;

    public AutenticacaoServiceTests()
    {
        repositorio.Setup(r => r.Usuario).Returns(usuarioRepository.Object);
        passwordHasher
            .Setup(h => h.HashPassword(It.IsAny<Usuario>(), It.IsAny<string>()))
            .Returns("HASH_GERADO");
        service = new AutenticacaoService(repositorio.Object, passwordHasher.Object);
    }

    [Fact]
    public async Task CadastrarCliente_EmailLivre_GravaClienteAtivoComSenhaEmHash()
    {
        // Arrange
        usuarioRepository.Setup(r => r.BuscarPorEmailAsync("ana@exemplo.com")).ReturnsAsync((Usuario?)null);
        Usuario? gravado = null;
        usuarioRepository
            .Setup(r => r.InserirAsync(It.IsAny<Usuario>()))
            .Callback<Usuario>(usuario => gravado = usuario)
            .ReturnsAsync(7);

        // Act
        var cliente = await service.CadastrarClienteAsync("  Ana Souza ", " ana@exemplo.com ", "segredo123");

        // Assert
        Assert.Equal(7, cliente.Id);
        Assert.NotNull(gravado);
        Assert.Equal("Ana Souza", gravado!.Nome);
        Assert.Equal("ana@exemplo.com", gravado.Email);
        Assert.Equal(PerfilIds.Cliente, gravado.IdPerfil);
        Assert.True(gravado.Ativo);
        Assert.Equal("HASH_GERADO", gravado.SenhaHash);
        Assert.DoesNotContain("segredo123", gravado.SenhaHash);
        passwordHasher.Verify(h => h.HashPassword(It.IsAny<Usuario>(), "segredo123"), Times.Once);
    }

    [Fact]
    public async Task CadastrarCliente_EmailJaCadastrado_LancaEmailJaCadastradoENaoGrava()
    {
        // Arrange
        usuarioRepository
            .Setup(r => r.BuscarPorEmailAsync("ana@exemplo.com"))
            .ReturnsAsync(new Usuario { Id = 3, Email = "ana@exemplo.com", IdPerfil = PerfilIds.Cliente });

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.CadastrarClienteAsync("Ana", "ana@exemplo.com", "segredo123"));

        // Assert
        Assert.Equal(BusinessErrors.EMAIL_JA_CADASTRADO, excecao.Erro);
        Assert.Equal(106, excecao.Erro.Code);
        usuarioRepository.Verify(r => r.InserirAsync(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task CadastrarCliente_RepositoryLancaRegistroDuplicado_LancaEmailJaCadastrado()
    {
        // Arrange: a verificação passa, mas o UNIQUE de ds_email recusa o INSERT (corrida entre requisições).
        usuarioRepository.Setup(r => r.BuscarPorEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario?)null);
        usuarioRepository
            .Setup(r => r.InserirAsync(It.IsAny<Usuario>()))
            .ThrowsAsync(new RegistroDuplicadoException("Duplicado", new Exception()));

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.CadastrarClienteAsync("Ana", "ana@exemplo.com", "segredo123"));

        // Assert
        Assert.Equal(BusinessErrors.EMAIL_JA_CADASTRADO, excecao.Erro);
    }
}
