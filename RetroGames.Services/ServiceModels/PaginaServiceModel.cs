namespace RetroGames.Services.ServiceModels;

/// <summary>Uma página de resultados e o total encontrado com o mesmo filtro.</summary>
public class PaginaServiceModel<T>
{
    public List<T> Itens { get; set; } = new();

    public int Pagina { get; set; }

    public int TamanhoPagina { get; set; }

    public int TotalItens { get; set; }
}
