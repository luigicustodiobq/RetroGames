using RetroGames.Model.Model;
using RetroGames.Services.ServiceModels;
using RetroGames.WebApi.ApiModels;

namespace RetroGames.WebApi.Mappers;

/// <summary>Conversões entre os modelos de jogo do Service e os ApiModels.</summary>
public static class JogoMapper
{
    public static FiltroJogoServiceModel ParaFiltro(this ListarJogoRequestApiModel request)
    {
        return new FiltroJogoServiceModel
        {
            Busca = request.Busca,
            IdPlataforma = request.IdPlataforma,
            IdGenero = request.IdGenero,
            Pagina = request.Pagina,
            TamanhoPagina = request.TamanhoPagina,
            IncluirInativos = request.IncluirInativos
        };
    }

    public static Jogo ParaModel(this InserirJogoRequestApiModel request)
    {
        return new Jogo
        {
            Nome = request.Nome ?? string.Empty,
            Descricao = request.Descricao,
            AnoLancamento = request.AnoLancamento,
            Preco = request.Preco,
            Estoque = request.Estoque,
            IdPlataforma = request.IdPlataforma,
            IdCondicaoJogo = request.IdCondicaoJogo
        };
    }

    /// <summary>O id vem da rota (PUT /api/Jogo/{id}), não do corpo.</summary>
    public static Jogo ParaModel(this AlterarJogoRequestApiModel request, int id)
    {
        return new Jogo
        {
            Id = id,
            Nome = request.Nome ?? string.Empty,
            Descricao = request.Descricao,
            AnoLancamento = request.AnoLancamento,
            Preco = request.Preco,
            Estoque = request.Estoque,
            Ativo = request.Ativo,
            IdPlataforma = request.IdPlataforma,
            IdCondicaoJogo = request.IdCondicaoJogo
        };
    }

    public static ListarJogoResponseApiModel ParaResponse(this JogoDetalheServiceModel detalhe)
    {
        return new ListarJogoResponseApiModel
        {
            Id = detalhe.Jogo.Id,
            Nome = detalhe.Jogo.Nome,
            AnoLancamento = detalhe.Jogo.AnoLancamento,
            Preco = detalhe.Jogo.Preco,
            Estoque = detalhe.Jogo.Estoque,
            Ativo = detalhe.Jogo.Ativo,
            NomePlataforma = detalhe.NomePlataforma,
            DescricaoCondicao = detalhe.DescricaoCondicao,
            Generos = detalhe.Generos.Select(genero => genero.Nome).ToList()
        };
    }

    public static ListarPorIdJogoResponseApiModel ParaResponsePorId(this JogoDetalheServiceModel detalhe)
    {
        return new ListarPorIdJogoResponseApiModel
        {
            Id = detalhe.Jogo.Id,
            Nome = detalhe.Jogo.Nome,
            Descricao = detalhe.Jogo.Descricao,
            AnoLancamento = detalhe.Jogo.AnoLancamento,
            Preco = detalhe.Jogo.Preco,
            Estoque = detalhe.Jogo.Estoque,
            Ativo = detalhe.Jogo.Ativo,
            IdPlataforma = detalhe.Jogo.IdPlataforma,
            NomePlataforma = detalhe.NomePlataforma,
            IdCondicaoJogo = detalhe.Jogo.IdCondicaoJogo,
            DescricaoCondicao = detalhe.DescricaoCondicao,
            Generos = detalhe.Generos.Select(genero => genero.ParaResponse()).ToList()
        };
    }

    public static PaginaResponseApiModel<ListarJogoResponseApiModel> ParaResponse(
        this PaginaServiceModel<JogoDetalheServiceModel> pagina)
    {
        return new PaginaResponseApiModel<ListarJogoResponseApiModel>
        {
            Itens = pagina.Itens.Select(detalhe => detalhe.ParaResponse()).ToList(),
            Pagina = pagina.Pagina,
            TamanhoPagina = pagina.TamanhoPagina,
            TotalItens = pagina.TotalItens,
            // Divisão arredondada para cima: 25 itens em páginas de 12 = 3 páginas.
            TotalPaginas = (int)Math.Ceiling(pagina.TotalItens / (double)pagina.TamanhoPagina)
        };
    }
}
