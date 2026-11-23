import { useEffect, useRef, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { obterMensagemErro } from '../../api/erros'
import { jogoApi } from '../../api/servicos'
import CapaJogo from '../../components/CapaJogo'
import { Carregando, EstadoVazio, MensagemErro } from '../../components/MensagensEstado'
import SeletorQuantidade from '../../components/SeletorQuantidade'
import { useAutenticacao } from '../../hooks/useAutenticacao'
import { useCarrinho } from '../../hooks/useCarrinho'
import { formatarPreco } from '../../lib/formato'

/** Detalhe de um jogo (rota "/jogos/:id"). */
export default function DetalheJogo() {
  const { id } = useParams()
  const [jogo, setJogo] = useState(null)
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState('')
  const [naoEncontrado, setNaoEncontrado] = useState(false)
  const [tentativa, setTentativa] = useState(0)
  const [quantidade, setQuantidade] = useState(1)
  const [mensagemAdicionado, setMensagemAdicionado] = useState('')
  const { ehAdministrador } = useAutenticacao()
  const { itens, adicionarItem, limiteDoItem } = useCarrinho()
  const temporizadorAviso = useRef(null)

  // Ao sair da página, cancela o temporizador do aviso.
  useEffect(() => () => window.clearTimeout(temporizadorAviso.current), [])

  useEffect(() => {
    let ativo = true
    setCarregando(true)
    setErro('')
    setNaoEncontrado(false)

    jogoApi
      .buscarPorId(id)
      .then((dados) => ativo && setJogo(dados))
      .catch((falha) => {
        if (!ativo) return
        // 404 (jogo inexistente ou inativo) e id inválido na URL (400) viram "não encontrado".
        const status = falha?.response?.status
        if (status === 404 || status === 400) {
          setNaoEncontrado(true)
        } else {
          setErro(obterMensagemErro(falha))
        }
      })
      .finally(() => ativo && setCarregando(false))

    return () => {
      ativo = false
    }
  }, [id, tentativa])

  if (carregando) {
    return <Carregando texto="Carregando jogo..." />
  }

  if (naoEncontrado) {
    return (
      <EstadoVazio
        icone="🔍"
        titulo="Jogo não encontrado"
        descricao="Este jogo não existe ou não está mais disponível no catálogo."
        textoAcao="Voltar ao catálogo"
        para="/"
      />
    )
  }

  if (erro) {
    return <MensagemErro mensagem={erro} aoTentarNovamente={() => setTentativa((n) => n + 1)} />
  }

  const esgotado = jogo.estoque === 0
  const quantidadeNoCarrinho = itens.find((item) => item.idJogo === jogo.id)?.quantidade ?? 0
  const limiteRestante = Math.max(0, limiteDoItem(jogo.estoque) - quantidadeNoCarrinho)
  const quantidadeEscolhida = Math.min(quantidade, Math.max(1, limiteRestante))

  function aoAdicionar() {
    adicionarItem(jogo, quantidadeEscolhida)
    setQuantidade(1)
    // Heurística #1 (visibilidade do status): confirmação imediata do que aconteceu.
    setMensagemAdicionado(
      `${quantidadeEscolhida} × ${jogo.nome} ${quantidadeEscolhida === 1 ? 'foi adicionado' : 'foram adicionados'} ao carrinho.`,
    )
    window.clearTimeout(temporizadorAviso.current)
    temporizadorAviso.current = window.setTimeout(() => setMensagemAdicionado(''), 5000)
  }

  return (
    <>
      {mensagemAdicionado && (
        <div className="rg-toast" role="status" aria-live="polite">
          <div className="d-flex align-items-start gap-2">
            <span aria-hidden="true">✅</span>
            <div className="flex-grow-1">
              <p className="mb-2">{mensagemAdicionado}</p>
              <Link to="/carrinho" className="btn btn-accent btn-sm">Ir para o carrinho</Link>
            </div>
            <button
              type="button"
              className="btn-close btn-close-white"
              aria-label="Fechar aviso"
              onClick={() => setMensagemAdicionado('')}
            />
          </div>
        </div>
      )}

      <nav aria-label="Trilha de navegação" className="mb-3">
        <ol className="breadcrumb mb-0">
          <li className="breadcrumb-item"><Link to="/">Catálogo</Link></li>
          <li className="breadcrumb-item active" aria-current="page">{jogo.nome}</li>
        </ol>
      </nav>

      <article className="row g-4">
        <div className="col-12 col-lg-6">
          <div className="card overflow-hidden">
            <CapaJogo nome={jogo.nome}>
              <span className="badge rg-badge-condicao rg-selo">{jogo.descricaoCondicao}</span>
              {esgotado && <span className="badge rg-badge-esgotado rg-selo rg-selo-direita">Esgotado</span>}
            </CapaJogo>
          </div>
        </div>

        <div className="col-12 col-lg-6">
          <div className="rg-painel h-100 d-flex flex-column">
            <p className="rg-eyebrow">{jogo.nomePlataforma}</p>
            <h1 className="rg-titulo-pagina mb-2">{jogo.nome}</h1>

            <div className="d-flex flex-wrap gap-1 mb-3">
              {jogo.generos.map((genero) => (
                <span key={genero.id} className="badge rg-badge-genero">{genero.nome}</span>
              ))}
            </div>

            <p className="text-body-secondary">
              {jogo.descricao || 'Este jogo ainda não tem descrição.'}
            </p>

            <dl className="row small mb-4">
              <dt className="col-5 text-body-secondary fw-normal">Plataforma</dt>
              <dd className="col-7">{jogo.nomePlataforma}</dd>
              <dt className="col-5 text-body-secondary fw-normal">Ano de lançamento</dt>
              <dd className="col-7">{jogo.anoLancamento}</dd>
              <dt className="col-5 text-body-secondary fw-normal">Condição</dt>
              <dd className="col-7">{jogo.descricaoCondicao}</dd>
              <dt className="col-5 text-body-secondary fw-normal">Disponibilidade</dt>
              <dd className="col-7">
                {esgotado ? 'Esgotado' : `${jogo.estoque} ${jogo.estoque === 1 ? 'unidade' : 'unidades'} em estoque`}
              </dd>
            </dl>

            <div className="mt-auto">
              <p className="rg-preco rg-preco-grande mb-3">{formatarPreco(jogo.preco)}</p>

              {esgotado ? (
                // Heurística #5 (prevenção de erros): sem estoque, a compra fica bloqueada e o motivo é explicado.
                <div className="alert alert-warning mb-3" role="note">
                  <strong>Esgotado.</strong> Este jogo está sem estoque no momento e não pode ser adicionado ao carrinho.
                </div>
              ) : (
                !ehAdministrador && (
                  <div className="d-flex flex-column flex-sm-row gap-2 mb-3">
                    <label htmlFor="detalhe-quantidade" className="visually-hidden">Quantidade</label>
                    <SeletorQuantidade
                      id="detalhe-quantidade"
                      valor={quantidadeEscolhida}
                      maximo={limiteRestante}
                      desabilitado={limiteRestante === 0}
                      aoMudar={(valor) => setQuantidade(Math.min(Math.max(1, valor || 1), Math.max(1, limiteRestante)))}
                    />
                    <button
                      type="button"
                      className="btn btn-primary flex-grow-1"
                      onClick={aoAdicionar}
                      disabled={limiteRestante === 0}
                    >
                      🛒 Adicionar ao carrinho
                    </button>
                  </div>
                )
              )}

              {!esgotado && !ehAdministrador && quantidadeNoCarrinho > 0 && (
                <p className="small text-body-secondary mb-3">
                  Você já tem {quantidadeNoCarrinho} no carrinho
                  {limiteRestante === 0 ? ' (limite atingido).' : '.'}{' '}
                  <Link to="/carrinho">Ver carrinho</Link>
                </p>
              )}

              <Link to="/" className="btn btn-outline-light">← Voltar ao catálogo</Link>
            </div>
          </div>
        </div>
      </article>
    </>
  )
}
