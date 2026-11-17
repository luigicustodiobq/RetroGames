namespace RetroGames.WebApi.ApiModels;

/// <summary>Condição de jogo devolvida pela API.</summary>
public class ListarCondicaoJogoResponseApiModel
{
    public int Id { get; set; }

    public string Descricao { get; set; } = string.Empty;
}
