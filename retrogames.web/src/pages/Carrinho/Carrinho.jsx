import { useState } from 'react'
import { Link } from 'react-router-dom'
import { EstadoVazio } from '../../components/MensagensEstado'
import SeletorQuantidade from '../../components/SeletorQuantidade'
import { useCarrinho } from '../../hooks/useCarrinho'
import { formatarPreco } from '../../lib/formato'

/** C03: carrinho de compras (rota "/carrinho", exige login). */
export default function Carrinho() {
  const { itens, totalItens, valorTotal, alterarQuantidade, removerItem, limiteDoItem } = useCarrinho()
  const [aviso, setAviso] = useState('')

  function remover(item) {
    removerItem(item.idJogo)
    // Heurística #3 (controle e liberdade): o usuário pode desfazer adicionando de novo pelo link.
    setAviso(`"${item.nome}" foi removido do carrinho.`)
  }

  function finalizarCompra() {
    setAviso('A finalização da compra estará disponível em breve.')
  }

  if (itens.length === 0) {
    return (
      <>
        <header className="mb-4">
          <p className="rg-eyebrow">Sua compra</p>
          <h1 className="rg-titulo-pagina">Carrinho</h1>
        </header>
        {aviso && <div className="alert alert-info" role="status">{aviso}</div>}
        <EstadoVazio
          icone="🛒"
          titulo="Seu carrinho está vazio"
          descricao="Explore o catálogo e adicione os clássicos que você quer jogar."
          textoAcao="Ver catálogo"
          para="/"
        />
      </>
    )
  }

  return (
    <>
      <header className="mb-4">
        <p className="rg-eyebrow">Sua compra</p>
        <h1 className="rg-titulo-pagina">Carrinho</h1>
        <p className="rg-subtitulo">
          {totalItens} {totalItens === 1 ? 'item' : 'itens'} · até 10 unidades por jogo, limitado ao estoque.
        </p>
      </header>

      {aviso && (
        <div className="alert alert-info d-flex justify-content-between align-items-center gap-2" role="status">
          <span>{aviso}</span>
          <button type="button" className="btn-close btn-close-white" aria-label="Fechar aviso" onClick={() => setAviso('')} />
        </div>
      )}

      <div className="row g-4">
        <div className="col-12 col-lg-8">
          <ul className="list-unstyled d-grid gap-3 mb-0" aria-label="Itens do carrinho">
            {itens.map((item) => (
              <li key={item.idJogo} className="rg-painel">
                <div className="row g-3 align-items-center">
                  <div className="col-12 col-md-5">
                    <Link to={`/jogos/${item.idJogo}`} className="fw-semibold text-decoration-none d-block">
                      {item.nome}
                    </Link>
                    <span className="rg-meta">
                      {item.nomePlataforma} · {formatarPreco(item.preco)} cada
                    </span>
                  </div>
                  <div className="col-7 col-md-4">
                    <SeletorQuantidade
                      id={`quantidade-${item.idJogo}`}
                      valor={item.quantidade}
                      maximo={limiteDoItem(item.estoque)}
                      rotulo={`Quantidade de ${item.nome}`}
                      aoMudar={(valor) => alterarQuantidade(item.idJogo, valor)}
                    />
                  </div>
                  <div className="col-5 col-md-3 text-end">
                    <span className="d-block fw-semibold">{formatarPreco(item.preco * item.quantidade)}</span>
                    <button
                      type="button"
                      className="btn btn-link btn-sm text-danger px-0"
                      onClick={() => remover(item)}
                      aria-label={`Remover ${item.nome} do carrinho`}
                    >
                      Remover
                    </button>
                  </div>
                </div>
              </li>
            ))}
          </ul>
        </div>

        <aside className="col-12 col-lg-4" aria-labelledby="resumo-titulo">
          {/* Heurística #6 (reconhecer em vez de lembrar): resumo sempre visível. */}
          <div className="rg-painel rg-resumo">
            <h2 id="resumo-titulo" className="h5 mb-3">Resumo do pedido</h2>
            <dl className="mb-0">
              {itens.map((item) => (
                <div key={item.idJogo} className="d-flex justify-content-between gap-2 small mb-1">
                  <dt className="fw-normal text-body-secondary text-truncate">{item.quantidade} × {item.nome}</dt>
                  <dd className="mb-0 text-nowrap">{formatarPreco(item.preco * item.quantidade)}</dd>
                </div>
              ))}
              <hr className="rg-pixel-divisor my-3" />
              <div className="d-flex justify-content-between align-items-baseline">
                <dt className="fw-semibold">Total</dt>
                <dd className="mb-0 rg-preco">{formatarPreco(valorTotal)}</dd>
              </div>
            </dl>
            <p className="small text-body-secondary mt-2">
              O valor final é confirmado com os preços atuais da loja no momento da compra.
            </p>
            <button type="button" className="btn btn-primary w-100" onClick={finalizarCompra}>
              Finalizar compra
            </button>
            <Link to="/" className="btn btn-outline-light w-100 mt-2">Continuar comprando</Link>
          </div>
        </aside>
      </div>
    </>
  )
}
