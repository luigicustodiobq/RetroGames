using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>Acesso a dados da tabela de domínio tb_situacao_pedido (somente leitura).</summary>
public interface ISituacaoPedidoRepository
{
    /// <summary>Todas as situações de pedido, na ordem das chaves fixas.</summary>
    Task<List<SituacaoPedido>> ListarAsync();
}
