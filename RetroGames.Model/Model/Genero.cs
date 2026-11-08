namespace RetroGames.Model.Model;

/// <summary>Gênero do jogo. Tabela tb_genero.</summary>
public class Genero
{
    // co_seq_genero
    public int Id { get; set; }

    // no_genero
    public string Nome { get; set; } = string.Empty;
}
