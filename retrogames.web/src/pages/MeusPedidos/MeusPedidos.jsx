import { useEffect, useState } from 'react'
import { useLocation } from 'react-router-dom'
import { obterMensagemErro } from '../../api/erros'
import { pedidoApi } from '../../api/servicos'
import { Carregando, EstadoVazio, MensagemErro } from '../../components/MensagensEstado'
import { formatarDataHora, formatarPreco } from '../../lib/formato'

// Cor e texto de apoio de cada situação (Heurística #2: linguagem do usuário).
const SITUACOES = {
  Pendente: { classe: 'rg-situacao-pendente', dica: 'Aguardando a confirmação do pagamento.' },
  Pago: { classe: 'rg-situacao-pago', dica: 'Pagamento confirmado. Estamos separando seu pedido.' },
  Enviado: { classe: 'rg-situacao-enviado', dica: 'Pedido a caminho do seu endereço.' },
  Cancelado: { classe: 'rg-situacao-cancelado', dica: 'Este pedido foi cancelado.' },
}

/** C05: pedidos do cliente logado (rota "/meus-pedidos", exige login). */
export default function MeusPedidos() {
  const location = useLocation()
  const idPedidoCriado = location.state?.idPedidoCriado

  const [pedidos, setPedidos] = useState([])
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState('')
  const [tentativa, setTentativa] = useState(0)

  useEffect(() => {
    let ativo = true
    setCarregando(true)
    setErro('')
    pedidoApi
      .listarMeus()
      .then((dados) => ativo && setPedidos(dados))
      .catch((falha) => ativo && setErro(obterMensagemErro(falha)))
      .finally(() => ativo && setCarregando(false))
    return () => {
      ativo = false
    }
  }, [tentativa])

  return (
    <>
      <header className="mb-4">
        <p className="rg-eyebrow">Minha conta</p>
        <h1 className="rg-titulo-pagina">Meus pedidos</h1>
        <p className="rg-subtitulo">Acompanhe suas compras, da mais recente para a mais antiga.</p>
      </header>

      {idPedidoCriado && (
        <div className="alert alert-success" role="status">
          <strong>Compra concluída!</strong> Seu pedido nº {idPedidoCriado} foi registrado e o estoque já foi reservado.
        </div>
      )}

      {carregando && <Carregando texto="Carregando seus pedidos..." />}
      {!carregando && erro && <MensagemErro mensagem={erro} aoTentarNovamente={() => setTentativa((n) => n + 1)} />}
      {!carregando && !erro && pedidos.length === 0 && (
        <EstadoVazio
          icone="📦"
          titulo="Você ainda não fez nenhum pedido"
          descricao="Quando você finalizar uma compra, ela aparecerá aqui com a situação atualizada."
          textoAcao="Explorar o catálogo"
          para="/"
        />
      )}

      {!carregando && !erro && pedidos.length > 0 && (
        <div className="d-grid gap-3">
          {pedidos.map((pedido) => {
            const situacao = SITUACOES[pedido.descricaoSituacao] ?? { classe: 'rg-badge-inativo', dica: '' }
            const quantidadeItens = pedido.itens.reduce((soma, item) => soma + item.quantidade, 0)
            return (
              <article
                key={pedido.id}
                className={`rg-painel ${pedido.id === idPedidoCriado ? 'border-success' : ''}`}
                aria-labelledby={`pedido-${pedido.id}`}
              >
                <div className="d-flex flex-column flex-md-row justify-content-between gap-2 mb-3">
                  <div>
                    <h2 id={`pedido-${pedido.id}`} className="h5 mb-1">Pedido nº {pedido.id}</h2>
                    <p className="rg-meta mb-0">
                      Feito em {formatarDataHora(pedido.dataPedido)} · {quantidadeItens}{' '}
                      {quantidadeItens === 1 ? 'item' : 'itens'}
                    </p>
                  </div>
                  <div className="text-md-end">
                    <span className={`badge ${situacao.classe} mb-1`}>{pedido.descricaoSituacao}</span>
                    <p className="rg-meta mb-0">{situacao.dica}</p>
                  </div>
                </div>

                <div className="table-responsive">
                  <table className="table table-sm">
                    <caption className="visually-hidden">Itens do pedido {pedido.id}</caption>
                    <thead>
                      <tr>
                        <th scope="col">Jogo</th>
                        <th scope="col" className="text-end">Qtd.</th>
                        <th scope="col" className="text-end">Preço pago</th>
                        <th scope="col" className="text-end">Subtotal</th>
                      </tr>
                    </thead>
                    <tbody>
                      {pedido.itens.map((item) => (
                        <tr key={item.idJogo}>
                          <td>{item.nomeJogo}</td>
                          <td className="text-end">{item.quantidade}</td>
                          <td className="text-end text-nowrap">{formatarPreco(item.valorUnitario)}</td>
                          <td className="text-end text-nowrap">{formatarPreco(item.subtotal)}</td>
                        </tr>
                      ))}
                    </tbody>
                    <tfoot>
                      <tr>
                        <th scope="row" colSpan={3} className="text-end border-0">Total</th>
                        <td className="text-end border-0 rg-preco text-nowrap">{formatarPreco(pedido.valorTotal)}</td>
                      </tr>
                    </tfoot>
                  </table>
                </div>
              </article>
            )
          })}
        </div>
      )}
    </>
  )
}
