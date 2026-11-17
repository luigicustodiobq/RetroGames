namespace RetroGames.WebApi.ApiModels;

/// <summary>Corpo de PUT /api/Plataforma/{id}. O id vem da rota.</summary>
public class AlterarPlataformaRequestApiModel
{
    public string? Nome { get; set; }

    public string? Fabricante { get; set; }

    public int AnoLancamento { get; set; }
}
