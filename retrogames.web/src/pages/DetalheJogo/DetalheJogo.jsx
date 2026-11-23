import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { obterMensagemErro } from '../../api/erros'
import { jogoApi } from '../../api/servicos'
import CapaJogo from '../../components/CapaJogo'
import { Carregando, EstadoVazio, MensagemErro } from '../../components/MensagensEstado'
import { formatarPreco } from '../../lib/formato'

/** Detalhe de um jogo (rota "/jogos/:id"). */
export default function DetalheJogo() {
  const { id } = useParams()
  const [jogo, setJogo] = useState(null)
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState('')
  const [naoEncontrado, setNaoEncontrado] = useState(false)
  const [tentativa, setTentativa] = useState(0)

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

  return (
    <>
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
              <Link to="/" className="btn btn-outline-light">← Voltar ao catálogo</Link>
            </div>
          </div>
        </div>
      </article>
    </>
  )
}
