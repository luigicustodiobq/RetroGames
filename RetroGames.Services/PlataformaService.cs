using RetroGames.Model.Model;
using RetroGames.Repository;
using RetroGames.Services.Erros;

namespace RetroGames.Services;

/// <summary>
/// Regras de negócio de plataformas. Não tem SQL nem conhece HTTP:
/// acessa os dados por repositorio.Plataforma e sinaliza erros com BusinessRuleException.
/// </summary>
public class PlataformaService : IPlataformaService
{
    private readonly IRepositorySet repositorio;

    public PlataformaService(IRepositorySet repositorio)
    {
        this.repositorio = repositorio;
    }

    public Task<List<Plataforma>> ListarAsync()
    {
        return repositorio.Plataforma.ListarAsync();
    }

    public async Task<Plataforma> BuscarPorIdAsync(int id)
    {
        return await repositorio.Plataforma.BuscarPorIdAsync(id)
            ?? throw new BusinessRuleException(BusinessErrors.PLATAFORMA_NAO_ENCONTRADA);
    }

    public async Task<Plataforma> InserirAsync(Plataforma plataforma)
    {
        NormalizarTextos(plataforma);

        if (await repositorio.Plataforma.ExisteComNomeAsync(plataforma.Nome))
        {
            throw new BusinessRuleException(BusinessErrors.PLATAFORMA_NOME_DUPLICADO);
        }

        try
        {
            plataforma.Id = await repositorio.Plataforma.InserirAsync(plataforma);
        }
        catch (RegistroDuplicadoException ex)
        {
            // Outra requisição gravou o mesmo nome entre a verificação e o INSERT.
            throw new BusinessRuleException(BusinessErrors.PLATAFORMA_NOME_DUPLICADO, ex);
        }

        return plataforma;
    }

    public async Task<Plataforma> AlterarAsync(Plataforma plataforma)
    {
        await BuscarPorIdAsync(plataforma.Id); // erro 100 se não existir

        NormalizarTextos(plataforma);

        // Ignora o próprio Id: manter o mesmo nome não é duplicidade.
        if (await repositorio.Plataforma.ExisteComNomeAsync(plataforma.Nome, plataforma.Id))
        {
            throw new BusinessRuleException(BusinessErrors.PLATAFORMA_NOME_DUPLICADO);
        }

        try
        {
            await repositorio.Plataforma.AlterarAsync(plataforma);
        }
        catch (RegistroDuplicadoException ex)
        {
            throw new BusinessRuleException(BusinessErrors.PLATAFORMA_NOME_DUPLICADO, ex);
        }

        return plataforma;
    }

    public async Task ExcluirAsync(int id)
    {
        await BuscarPorIdAsync(id); // erro 100 se não existir

        if (await repositorio.Plataforma.PossuiJogosAsync(id))
        {
            throw new BusinessRuleException(BusinessErrors.PLATAFORMA_POSSUI_JOGOS);
        }

        await repositorio.Plataforma.ExcluirAsync(id);
    }

    /// <summary>Remove espaços nas pontas de nome e fabricante.</summary>
    private static void NormalizarTextos(Plataforma plataforma)
    {
        plataforma.Nome = plataforma.Nome.Trim();
        plataforma.Fabricante = plataforma.Fabricante.Trim();
    }
}
