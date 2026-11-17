using RetroGames.Model.Model;

namespace RetroGames.Services.ServiceModels;

/// <summary>
/// Um jogo com os dados das tabelas relacionadas, montado no Service com LINQ
/// (o Repository lê uma tabela por consulta).
/// </summary>
public class JogoDetalheServiceModel
{
    public Jogo Jogo { get; set; } = new();

    public string NomePlataforma { get; set; } = string.Empty;

    public string DescricaoCondicao { get; set; } = string.Empty;

    public List<Genero> Generos { get; set; } = new();
}
