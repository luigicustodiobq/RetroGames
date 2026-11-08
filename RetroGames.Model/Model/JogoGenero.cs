namespace RetroGames.Model.Model;

/// <summary>Associação entre jogo e gênero (N:N). Tabela tb_jogo_genero.</summary>
public class JogoGenero
{
    // co_jogo
    public int IdJogo { get; set; }

    // co_genero
    public int IdGenero { get; set; }
}
