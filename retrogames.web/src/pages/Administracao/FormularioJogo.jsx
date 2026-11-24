import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { obterErrosDosCampos, obterMensagemErro } from '../../api/erros'
import { condicaoJogoApi, generoApi, jogoApi, plataformaApi } from '../../api/servicos'
import { Carregando, MensagemErro } from '../../components/MensagensEstado'
import { useTituloPagina } from '../../hooks/useTituloPagina'

const FORMULARIO_VAZIO = {
  nome: '',
  descricao: '',
  anoLancamento: '',
  preco: '',
  estoque: '',
  idPlataforma: '',
  idCondicaoJogo: '',
  idsGeneros: [],
  ativo: true,
}

/** Inclusão (/admin/jogos/novo) e edição (/admin/jogos/:id/editar) de jogo. */
export default function FormularioJogo() {
  const { id } = useParams()
  const ehEdicao = Boolean(id)
  useTituloPagina(ehEdicao ? 'Editar jogo' : 'Novo jogo')
  const navigate = useNavigate()

  const [plataformas, setPlataformas] = useState([])
  const [condicoes, setCondicoes] = useState([])
  const [generos, setGeneros] = useState([])
  const [formulario, setFormulario] = useState(FORMULARIO_VAZIO)

  const [carregando, setCarregando] = useState(true)
  const [erroCarga, setErroCarga] = useState('')
  const [tentativa, setTentativa] = useState(0)
  const [salvando, setSalvando] = useState(false)
  const [errosCampos, setErrosCampos] = useState({})
  const [erroGeral, setErroGeral] = useState('')

  useEffect(() => {
    let ativo = true
    setCarregando(true)
    setErroCarga('')

    Promise.all([
      plataformaApi.listar(),
      condicaoJogoApi.listarCondicoes(),
      generoApi.listar(),
      ehEdicao ? jogoApi.buscarPorId(id) : Promise.resolve(null),
    ])
      .then(([listaPlataformas, listaCondicoes, listaGeneros, jogo]) => {
        if (!ativo) return
        setPlataformas(listaPlataformas)
        setCondicoes(listaCondicoes)
        setGeneros(listaGeneros)
        if (jogo) {
          setFormulario({
            nome: jogo.nome,
            descricao: jogo.descricao ?? '',
            anoLancamento: String(jogo.anoLancamento),
            preco: String(jogo.preco),
            estoque: String(jogo.estoque),
            idPlataforma: String(jogo.idPlataforma),
            idCondicaoJogo: String(jogo.idCondicaoJogo),
            idsGeneros: jogo.generos.map((genero) => genero.id),
            ativo: jogo.ativo,
          })
        }
      })
      .catch((falha) => ativo && setErroCarga(obterMensagemErro(falha)))
      .finally(() => ativo && setCarregando(false))

    return () => {
      ativo = false
    }
  }, [id, ehEdicao, tentativa])

  function alterarCampo(evento) {
    const { name, value } = evento.target
    setFormulario((atual) => ({ ...atual, [name]: value }))
    setErrosCampos((atuais) => ({ ...atuais, [name]: undefined }))
  }

  function alternarGenero(idGenero) {
    setFormulario((atual) => ({
      ...atual,
      idsGeneros: atual.idsGeneros.includes(idGenero)
        ? atual.idsGeneros.filter((idAtual) => idAtual !== idGenero)
        : [...atual.idsGeneros, idGenero],
    }))
    setErrosCampos((atuais) => ({ ...atuais, idsGeneros: undefined }))
  }

  async function aoSalvar(evento) {
    evento.preventDefault()
    setSalvando(true)
    setErrosCampos({})
    setErroGeral('')

    // Os campos do formulário são texto: os numéricos vão convertidos com Number().
    const dados = {
      nome: formulario.nome,
      descricao: formulario.descricao.trim() || null,
      anoLancamento: Number(formulario.anoLancamento),
      preco: Number(formulario.preco),
      estoque: Number(formulario.estoque),
      idPlataforma: Number(formulario.idPlataforma),
      idCondicaoJogo: Number(formulario.idCondicaoJogo),
      idsGeneros: formulario.idsGeneros,
    }

    try {
      if (ehEdicao) {
        await jogoApi.alterar(id, { ...dados, ativo: formulario.ativo })
      } else {
        await jogoApi.inserir(dados)
      }
      navigate('/admin/jogos')
    } catch (falha) {
      setErrosCampos(obterErrosDosCampos(falha))
      setErroGeral(obterMensagemErro(falha))
      setSalvando(false)
      window.scrollTo({ top: 0, behavior: 'smooth' })
    }
  }

  if (carregando) {
    return <Carregando texto="Carregando formulário..." />
  }

  if (erroCarga) {
    return <MensagemErro mensagem={erroCarga} aoTentarNovamente={() => setTentativa((n) => n + 1)} />
  }

  const classe = (base, nome) => `${base} ${errosCampos[nome] ? 'is-invalid' : ''}`

  return (
    <>
      <nav aria-label="Trilha de navegação" className="mb-3">
        <ol className="breadcrumb mb-0">
          <li className="breadcrumb-item"><Link to="/admin/jogos">Jogos</Link></li>
          <li className="breadcrumb-item active" aria-current="page">{ehEdicao ? 'Editar' : 'Novo'}</li>
        </ol>
      </nav>

      <header className="mb-4">
        <p className="rg-eyebrow">Administração</p>
        <h1 className="rg-titulo-pagina">{ehEdicao ? `Editar ${formulario.nome}` : 'Novo jogo'}</h1>
        <p className="rg-subtitulo">Campos com * são obrigatórios.</p>
      </header>

      {erroGeral && <div className="alert alert-danger" role="alert">{erroGeral}</div>}

      <form className="rg-painel" onSubmit={aoSalvar} noValidate>
        <div className="row g-3">
          <div className="col-12 col-md-8">
            <label htmlFor="jogo-nome" className="form-label">Nome *</label>
            <input
              id="jogo-nome"
              name="nome"
              className={classe('form-control', 'nome')}
              value={formulario.nome}
              onChange={alterarCampo}
              maxLength={150}
            />
            <div className="invalid-feedback">{errosCampos.nome}</div>
          </div>

          <div className="col-12 col-md-4">
            <label htmlFor="jogo-ano" className="form-label">Ano de lançamento *</label>
            <input
              id="jogo-ano"
              name="anoLancamento"
              type="number"
              inputMode="numeric"
              className={classe('form-control', 'anoLancamento')}
              value={formulario.anoLancamento}
              onChange={alterarCampo}
            />
            <div className="invalid-feedback">{errosCampos.anoLancamento}</div>
          </div>

          <div className="col-12">
            <label htmlFor="jogo-descricao" className="form-label">Descrição</label>
            <textarea
              id="jogo-descricao"
              name="descricao"
              rows={3}
              className={classe('form-control', 'descricao')}
              value={formulario.descricao}
              onChange={alterarCampo}
            />
            <div className="form-text">Opcional. Conte a história e o estado do exemplar.</div>
            <div className="invalid-feedback">{errosCampos.descricao}</div>
          </div>

          <div className="col-12 col-md-6 col-lg-3">
            <label htmlFor="jogo-preco" className="form-label">Preço (R$) *</label>
            <input
              id="jogo-preco"
              name="preco"
              type="number"
              step="0.01"
              min="0"
              inputMode="decimal"
              className={classe('form-control', 'preco')}
              value={formulario.preco}
              onChange={alterarCampo}
            />
            <div className="invalid-feedback">{errosCampos.preco}</div>
          </div>

          <div className="col-12 col-md-6 col-lg-3">
            <label htmlFor="jogo-estoque" className="form-label">Estoque *</label>
            <input
              id="jogo-estoque"
              name="estoque"
              type="number"
              min="0"
              inputMode="numeric"
              className={classe('form-control', 'estoque')}
              value={formulario.estoque}
              onChange={alterarCampo}
            />
            <div className="invalid-feedback">{errosCampos.estoque}</div>
          </div>

          <div className="col-12 col-md-6 col-lg-3">
            <label htmlFor="jogo-plataforma" className="form-label">Plataforma *</label>
            <select
              id="jogo-plataforma"
              name="idPlataforma"
              className={classe('form-select', 'idPlataforma')}
              value={formulario.idPlataforma}
              onChange={alterarCampo}
            >
              <option value="">Selecione...</option>
              {plataformas.map((plataforma) => (
                <option key={plataforma.id} value={plataforma.id}>{plataforma.nome}</option>
              ))}
            </select>
            <div className="invalid-feedback">{errosCampos.idPlataforma}</div>
          </div>

          <div className="col-12 col-md-6 col-lg-3">
            <label htmlFor="jogo-condicao" className="form-label">Condição *</label>
            <select
              id="jogo-condicao"
              name="idCondicaoJogo"
              className={classe('form-select', 'idCondicaoJogo')}
              value={formulario.idCondicaoJogo}
              onChange={alterarCampo}
            >
              <option value="">Selecione...</option>
              {condicoes.map((condicao) => (
                <option key={condicao.id} value={condicao.id}>{condicao.descricao}</option>
              ))}
            </select>
            <div className="invalid-feedback">{errosCampos.idCondicaoJogo}</div>
          </div>

          <fieldset className="col-12">
            <legend className="form-label fs-6">Gêneros * <span className="text-body-secondary">(marque ao menos um)</span></legend>
            <div className={`row row-cols-2 row-cols-md-3 row-cols-lg-5 g-2 ${errosCampos.idsGeneros ? 'is-invalid' : ''}`}>
              {generos.map((genero) => (
                <div key={genero.id} className="col">
                  <div className="form-check">
                    <input
                      id={`genero-${genero.id}`}
                      type="checkbox"
                      className={classe('form-check-input', 'idsGeneros')}
                      checked={formulario.idsGeneros.includes(genero.id)}
                      onChange={() => alternarGenero(genero.id)}
                    />
                    <label htmlFor={`genero-${genero.id}`} className="form-check-label">{genero.nome}</label>
                  </div>
                </div>
              ))}
            </div>
            {errosCampos.idsGeneros && (
              <div className="invalid-feedback d-block">{errosCampos.idsGeneros}</div>
            )}
          </fieldset>
        </div>

        <hr className="my-4" />

        <div className="d-flex flex-column flex-sm-row gap-2 justify-content-end">
          <Link to="/admin/jogos" className="btn btn-outline-light">Cancelar</Link>
          <button type="submit" className="btn btn-primary" disabled={salvando}>
            {salvando ? 'Salvando...' : ehEdicao ? 'Salvar alterações' : 'Cadastrar jogo'}
          </button>
        </div>
      </form>
    </>
  )
}
