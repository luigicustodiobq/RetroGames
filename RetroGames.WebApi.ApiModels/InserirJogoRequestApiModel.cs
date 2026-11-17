namespace RetroGames.WebApi.ApiModels;

/// <summary>Corpo de POST /api/Jogo. O formato é conferido pelo validator.</summary>
public class InserirJogoRequestApiModel
{
    public string? Nome { get; set; }

    /// <summary>Opcional.</summary>
    public string? Descricao { get; set; }

    public int AnoLancamento { get; set; }

    public decimal Preco { get; set; }

    public int Estoque { get; set; }

    public int IdPlataforma { get; set; }

    public int IdCondicaoJogo { get; set; }

    /// <summary>Ao menos um gênero (RN05).</summary>
    public List<int>? IdsGeneros { get; set; }
}
