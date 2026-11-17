namespace RetroGames.WebApi.ApiModels;

/// <summary>Corpo de POST /api/Plataforma. O formato é conferido pelo validator.</summary>
public class InserirPlataformaRequestApiModel
{
    public string? Nome { get; set; }

    public string? Fabricante { get; set; }

    public int AnoLancamento { get; set; }
}
