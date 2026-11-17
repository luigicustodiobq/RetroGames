namespace RetroGames.WebApi.ApiModels;

/// <summary>Filtros e paginação de GET /api/Jogo, lidos da query string.</summary>
public class ListarJogoRequestApiModel
{
    /// <summary>Parte do nome do jogo.</summary>
    public string? Busca { get; set; }

    public int? IdPlataforma { get; set; }

    public int? IdGenero { get; set; }

    public int Pagina { get; set; } = 1;

    public int TamanhoPagina { get; set; } = 12;
}
