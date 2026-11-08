namespace RetroGames.Model.Model;

/// <summary>Jogo à venda. Tabela tb_jogo.</summary>
public class Jogo
{
    // co_seq_jogo
    public int Id { get; set; }

    // no_jogo
    public string Nome { get; set; } = string.Empty;

    // ds_jogo (única coluna opcional do modelo de dados)
    public string? Descricao { get; set; }

    // nu_ano_lancamento
    public int AnoLancamento { get; set; }

    // vl_preco
    public decimal Preco { get; set; }

    // qt_estoque
    public int Estoque { get; set; }

    // st_ativo
    public bool Ativo { get; set; }

    // dt_cadastro
    public DateTime DataCadastro { get; set; }

    // co_plataforma
    public int IdPlataforma { get; set; }

    // co_condicao_jogo
    public int IdCondicaoJogo { get; set; }
}
