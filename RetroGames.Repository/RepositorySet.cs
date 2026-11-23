namespace RetroGames.Repository;

/// <summary>
/// Reúne os repositórios recebidos no construtor (pela injeção de dependência da API).
/// Receber as interfaces, e não criar as classes aqui, permite substituir qualquer
/// repositório por um mock nos testes.
/// </summary>
public class RepositorySet : IRepositorySet
{
    public RepositorySet(
        IPlataformaRepository plataforma,
        IJogoRepository jogo,
        IJogoGeneroRepository jogoGenero,
        IGeneroRepository genero,
        ICondicaoJogoRepository condicaoJogo,
        IUsuarioRepository usuario,
        ISituacaoPedidoRepository situacaoPedido,
        IPedidoRepository pedido,
        IItemPedidoRepository itemPedido)
    {
        Plataforma = plataforma;
        Jogo = jogo;
        JogoGenero = jogoGenero;
        Genero = genero;
        CondicaoJogo = condicaoJogo;
        Usuario = usuario;
        SituacaoPedido = situacaoPedido;
        Pedido = pedido;
        ItemPedido = itemPedido;
    }

    public IPlataformaRepository Plataforma { get; }
    public IJogoRepository Jogo { get; }
    public IJogoGeneroRepository JogoGenero { get; }
    public IGeneroRepository Genero { get; }
    public ICondicaoJogoRepository CondicaoJogo { get; }
    public IUsuarioRepository Usuario { get; }
    public ISituacaoPedidoRepository SituacaoPedido { get; }
    public IPedidoRepository Pedido { get; }
    public IItemPedidoRepository ItemPedido { get; }
}
