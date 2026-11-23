using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>Acesso a dados da tabela tb_pedido.</summary>
public interface IPedidoRepository
{
    /// <summary>Grava um novo pedido e devolve o id gerado pelo banco.</summary>
    Task<int> InserirAsync(Pedido pedido);

    /// <summary>Os pedidos de um usuário, do mais recente para o mais antigo.</summary>
    Task<List<Pedido>> ListarPorUsuarioAsync(int idUsuario);

    /// <summary>O pedido com o id informado, ou null se não existir.</summary>
    Task<Pedido?> BuscarPorIdAsync(int id);
}
