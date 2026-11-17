using System.Transactions;
using RetroGames.Model.Model;
using RetroGames.Repository;
using RetroGames.Services.Erros;
using RetroGames.Services.ServiceModels;

namespace RetroGames.Services;

/// <summary>
/// Regras de negócio de jogos. Plataforma, condição e gêneros são combinados aqui com LINQ,
/// sempre com um número fixo de consultas (nunca uma consulta por jogo).
/// </summary>
public class JogoService : IJogoService
{
    private readonly IRepositorySet repositorio;

    public JogoService(IRepositorySet repositorio)
    {
        this.repositorio = repositorio;
    }

    public async Task<PaginaServiceModel<JogoDetalheServiceModel>> ListarAsync(FiltroJogoServiceModel filtro)
    {
        // Evita OFFSET negativo no SQL se vier página ou tamanho menor que 1.
        var pagina = Math.Max(1, filtro.Pagina);
        var tamanhoPagina = Math.Max(1, filtro.TamanhoPagina);

        // Filtro por gênero: os ids dos jogos do gênero saem de tb_jogo_genero, selecionados com LINQ.
        List<int>? idsJogos = null;
        if (filtro.IdGenero.HasValue)
        {
            var vinculos = await repositorio.JogoGenero.ListarTodosAsync();
            idsJogos = vinculos
                .Where(vinculo => vinculo.IdGenero == filtro.IdGenero.Value)
                .Select(vinculo => vinculo.IdJogo)
                .Distinct()
                .ToList();

            if (idsJogos.Count == 0)
            {
                // Nenhum jogo tem o gênero: página vazia, sem erro.
                return new PaginaServiceModel<JogoDetalheServiceModel>
                {
                    Pagina = pagina,
                    TamanhoPagina = tamanhoPagina,
                    TotalItens = 0
                };
            }
        }

        // Por enquanto só jogos ativos; a listagem de inativos para o administrador entra na aula 6.
        var (jogos, total) = await repositorio.Jogo.ListarAsync(
            filtro.Busca, filtro.IdPlataforma, idsJogos, incluirInativos: false, pagina, tamanhoPagina);

        return new PaginaServiceModel<JogoDetalheServiceModel>
        {
            Itens = await MontarDetalhesAsync(jogos),
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalItens = total
        };
    }

    public async Task<JogoDetalheServiceModel> BuscarPorIdAsync(int id)
    {
        var jogo = await BuscarJogoExistenteAsync(id);
        var detalhes = await MontarDetalhesAsync(new List<Jogo> { jogo });
        return detalhes[0];
    }

    public async Task<JogoDetalheServiceModel> InserirAsync(Jogo jogo, IReadOnlyCollection<int> idsGeneros)
    {
        jogo.Nome = jogo.Nome.Trim();
        jogo.Ativo = true;
        var generosDistintos = idsGeneros.Distinct().ToList();

        await ValidarRelacionamentosAsync(jogo, generosDistintos);

        // Jogo e vínculos são gravados juntos: se algo falhar, nada fica gravado.
        using (var transacao = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            jogo.Id = await repositorio.Jogo.InserirAsync(jogo);
            await repositorio.JogoGenero.InserirAsync(jogo.Id, generosDistintos);
            transacao.Complete();
        }

        return await BuscarPorIdAsync(jogo.Id);
    }

    public async Task<JogoDetalheServiceModel> AlterarAsync(Jogo jogo, IReadOnlyCollection<int> idsGeneros)
    {
        await BuscarJogoExistenteAsync(jogo.Id); // erro 103 se não existir

        jogo.Nome = jogo.Nome.Trim();
        var generosDistintos = idsGeneros.Distinct().ToList();

        await ValidarRelacionamentosAsync(jogo, generosDistintos);

        using (var transacao = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            await repositorio.Jogo.AlterarAsync(jogo);
            await repositorio.JogoGenero.RemoverPorJogoAsync(jogo.Id);
            await repositorio.JogoGenero.InserirAsync(jogo.Id, generosDistintos);
            transacao.Complete();
        }

        return await BuscarPorIdAsync(jogo.Id);
    }

    public async Task DesativarAsync(int id)
    {
        await BuscarJogoExistenteAsync(id); // erro 103 se não existir
        await repositorio.Jogo.DesativarAsync(id);
    }

    /// <summary>O jogo do id (ativo ou não); erro 103 se não existir.</summary>
    private async Task<Jogo> BuscarJogoExistenteAsync(int id)
    {
        return await repositorio.Jogo.BuscarPorIdAsync(id)
            ?? throw new BusinessRuleException(BusinessErrors.JOGO_NAO_ENCONTRADO);
    }

    /// <summary>Confere se plataforma, condição e todos os gêneros informados existem.</summary>
    private async Task ValidarRelacionamentosAsync(Jogo jogo, List<int> idsGeneros)
    {
        if (await repositorio.Plataforma.BuscarPorIdAsync(jogo.IdPlataforma) is null)
        {
            throw new BusinessRuleException(BusinessErrors.PLATAFORMA_NAO_ENCONTRADA);
        }

        var condicoes = await repositorio.CondicaoJogo.ListarAsync();
        if (!condicoes.Any(condicao => condicao.Id == jogo.IdCondicaoJogo))
        {
            throw new BusinessRuleException(BusinessErrors.CONDICAO_NAO_ENCONTRADA);
        }

        var idsGenerosExistentes = (await repositorio.Genero.ListarAsync())
            .Select(genero => genero.Id)
            .ToHashSet();
        if (idsGeneros.Count == 0 || !idsGeneros.All(idsGenerosExistentes.Contains))
        {
            throw new BusinessRuleException(BusinessErrors.GENERO_NAO_ENCONTRADO);
        }
    }

    /// <summary>
    /// Monta o detalhe de uma lista de jogos com 4 consultas, qualquer que seja o tamanho da lista:
    /// plataformas, condições, gêneros e os vínculos dos jogos da lista.
    /// </summary>
    private async Task<List<JogoDetalheServiceModel>> MontarDetalhesAsync(List<Jogo> jogos)
    {
        if (jogos.Count == 0)
        {
            return new List<JogoDetalheServiceModel>();
        }

        var idsJogos = jogos.Select(jogo => jogo.Id).ToList();

        var plataformas = (await repositorio.Plataforma.ListarAsync()).ToDictionary(p => p.Id);
        var condicoes = (await repositorio.CondicaoJogo.ListarAsync()).ToDictionary(c => c.Id);
        var generos = (await repositorio.Genero.ListarAsync()).ToDictionary(g => g.Id);
        var vinculos = await repositorio.JogoGenero.ListarPorJogosAsync(idsJogos);

        // Gêneros de cada jogo, agrupados pelo id do jogo.
        var generosPorJogo = vinculos
            .Where(vinculo => generos.ContainsKey(vinculo.IdGenero))
            .GroupBy(vinculo => vinculo.IdJogo)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => grupo.Select(vinculo => generos[vinculo.IdGenero]).OrderBy(g => g.Nome).ToList());

        return jogos
            .Select(jogo => new JogoDetalheServiceModel
            {
                Jogo = jogo,
                NomePlataforma = plataformas.TryGetValue(jogo.IdPlataforma, out var plataforma)
                    ? plataforma.Nome
                    : string.Empty,
                DescricaoCondicao = condicoes.TryGetValue(jogo.IdCondicaoJogo, out var condicao)
                    ? condicao.Descricao
                    : string.Empty,
                Generos = generosPorJogo.TryGetValue(jogo.Id, out var generosDoJogo)
                    ? generosDoJogo
                    : new List<Genero>()
            })
            .ToList();
    }
}
