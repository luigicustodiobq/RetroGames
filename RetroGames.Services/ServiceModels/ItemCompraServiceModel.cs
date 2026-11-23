namespace RetroGames.Services.ServiceModels;

/// <summary>
/// Um item enviado pelo carrinho na finalização da compra (C04).
/// Só o jogo e a quantidade: o preço é sempre lido do banco pelo Service.
/// </summary>
public class ItemCompraServiceModel
{
    public int IdJogo { get; set; }

    public int Quantidade { get; set; }
}
