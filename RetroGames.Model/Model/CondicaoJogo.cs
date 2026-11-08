namespace RetroGames.Model.Model;

/// <summary>Condição de conservação do jogo. Tabela tb_condicao_jogo.</summary>
public class CondicaoJogo
{
    // co_condicao_jogo
    public int Id { get; set; }

    // ds_condicao_jogo
    public string Descricao { get; set; } = string.Empty;
}
