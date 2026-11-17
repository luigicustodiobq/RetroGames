namespace RetroGames.WebApi.ApiModels;

/// <summary>Jogo completo, com os ids usados no formulário de edição.</summary>
public class ListarPorIdJogoResponseApiModel
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public int AnoLancamento { get; set; }

    public decimal Preco { get; set; }

    public int Estoque { get; set; }

    public bool Ativo { get; set; }

    public int IdPlataforma { get; set; }

    public string NomePlataforma { get; set; } = string.Empty;

    public int IdCondicaoJogo { get; set; }

    public string DescricaoCondicao { get; set; } = string.Empty;

    /// <summary>Gêneros do jogo, com id e nome.</summary>
    public List<ListarGeneroResponseApiModel> Generos { get; set; } = new();
}
