using Microsoft.AspNetCore.Identity;
using RetroGames.Model.Constantes;
using RetroGames.Model.Model;
using RetroGames.Repository;
using RetroGames.Services.Erros;

namespace RetroGames.Services;

/// <summary>
/// Autenticação por e-mail e senha. A senha nunca é gravada nem comparada em texto:
/// o PasswordHasher gera o hash (com salt) e confere a senha digitada contra ele.
/// </summary>
public class AutenticacaoService : IAutenticacaoService
{
    private readonly IRepositorySet repositorio;
    private readonly IPasswordHasher<Usuario> passwordHasher;

    public AutenticacaoService(IRepositorySet repositorio, IPasswordHasher<Usuario> passwordHasher)
    {
        this.repositorio = repositorio;
        this.passwordHasher = passwordHasher;
    }

    public async Task<Usuario?> AutenticarAsync(string email, string senha)
    {
        var usuario = await repositorio.Usuario.BuscarPorEmailAsync(email.Trim());
        if (usuario is null || !usuario.Ativo)
        {
            return null;
        }

        var resultado = passwordHasher.VerifyHashedPassword(usuario, usuario.SenhaHash, senha);
        return resultado == PasswordVerificationResult.Failed ? null : usuario;
    }

    public async Task GarantirAdministradorInicialAsync(string email, string senha)
    {
        if (await repositorio.Usuario.ExisteAdministradorAsync())
        {
            return; // Já existe: subir a API de novo não duplica o administrador.
        }

        var administrador = new Usuario
        {
            Nome = "Administrador",
            Email = email.Trim(),
            IdPerfil = PerfilIds.Administrador
        };
        administrador.SenhaHash = passwordHasher.HashPassword(administrador, senha);

        await repositorio.Usuario.InserirAsync(administrador);
    }

    public async Task<Usuario> CadastrarClienteAsync(string nome, string email, string senha)
    {
        var emailNormalizado = email.Trim();

        if (await repositorio.Usuario.BuscarPorEmailAsync(emailNormalizado) is not null)
        {
            throw new BusinessRuleException(BusinessErrors.EMAIL_JA_CADASTRADO);
        }

        var cliente = new Usuario
        {
            Nome = nome.Trim(),
            Email = emailNormalizado,
            Ativo = true,
            IdPerfil = PerfilIds.Cliente
        };
        cliente.SenhaHash = passwordHasher.HashPassword(cliente, senha);

        try
        {
            cliente.Id = await repositorio.Usuario.InserirAsync(cliente);
        }
        catch (RegistroDuplicadoException ex)
        {
            // Outra requisição cadastrou o mesmo e-mail entre a verificação e o INSERT (UNIQUE em ds_email).
            throw new BusinessRuleException(BusinessErrors.EMAIL_JA_CADASTRADO, ex);
        }

        return cliente;
    }
}
