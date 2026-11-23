namespace RetroGames.Model.Model;

/// <summary>Situação de um pedido. Tabela tb_situacao_pedido.</summary>
public class SituacaoPedido
{
    // co_situacao_pedido
    public int Id { get; set; }

    // ds_situacao_pedido
    public string Descricao { get; set; } = string.Empty;
}
