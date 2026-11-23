import { useEffect, useRef, useState } from 'react'
import { obterErrosDosCampos, obterMensagemErro } from '../../api/erros'
import { plataformaApi } from '../../api/servicos'
import { Carregando, EstadoVazio, MensagemErro } from '../../components/MensagensEstado'

const FORMULARIO_VAZIO = { nome: '', fabricante: '', anoLancamento: '' }

/** Administração de plataformas (rota "/admin/plataformas"): listar, incluir, editar e excluir. */
export default function Plataforma() {
  const [plataformas, setPlataformas] = useState([])
  const [carregando, setCarregando] = useState(true)
  const [erroLista, setErroLista] = useState('')
  const [tentativa, setTentativa] = useState(0)

  const [formulario, setFormulario] = useState(FORMULARIO_VAZIO)
  const [idEmEdicao, setIdEmEdicao] = useState(null)
  const [errosCampos, setErrosCampos] = useState({})
  const [erroGeral, setErroGeral] = useState('')
  const [sucesso, setSucesso] = useState('')
  const [salvando, setSalvando] = useState(false)
  const [idExcluindo, setIdExcluindo] = useState(null)
  const campoNome = useRef(null)

  useEffect(() => {
    let ativo = true
    setCarregando(true)
    setErroLista('')
    plataformaApi
      .listar()
      .then((dados) => ativo && setPlataformas(dados))
      .catch((falha) => ativo && setErroLista(obterMensagemErro(falha)))
      .finally(() => ativo && setCarregando(false))
    return () => {
      ativo = false
    }
  }, [tentativa])

  const recarregar = () => setTentativa((n) => n + 1)

  function alterarCampo(evento) {
    const { name, value } = evento.target
    setFormulario((atual) => ({ ...atual, [name]: value }))
    // O erro do campo some assim que o usuário começa a corrigi-lo.
    setErrosCampos((atuais) => ({ ...atuais, [name]: undefined }))
  }

  function limparFormulario() {
    setFormulario(FORMULARIO_VAZIO)
    setIdEmEdicao(null)
    setErrosCampos({})
    setErroGeral('')
  }

  function editar(plataforma) {
    setFormulario({
      nome: plataforma.nome,
      fabricante: plataforma.fabricante,
      anoLancamento: String(plataforma.anoLancamento),
    })
    setIdEmEdicao(plataforma.id)
    setErrosCampos({})
    setErroGeral('')
    setSucesso('')
    campoNome.current?.focus()
  }

  async function aoSalvar(evento) {
    evento.preventDefault()
    setSalvando(true)
    setErrosCampos({})
    setErroGeral('')
    setSucesso('')

    const dados = {
      nome: formulario.nome,
      fabricante: formulario.fabricante,
      anoLancamento: Number(formulario.anoLancamento),
    }

    try {
      if (idEmEdicao) {
        await plataformaApi.alterar(idEmEdicao, dados)
        setSucesso(`Plataforma "${dados.nome.trim()}" alterada.`)
      } else {
        await plataformaApi.inserir(dados)
        setSucesso(`Plataforma "${dados.nome.trim()}" cadastrada.`)
      }
      limparFormulario()
      recarregar()
    } catch (falha) {
      setErrosCampos(obterErrosDosCampos(falha))
      setErroGeral(obterMensagemErro(falha))
    } finally {
      setSalvando(false)
    }
  }

  async function excluir(plataforma) {
    if (!window.confirm(`Excluir a plataforma "${plataforma.nome}"? Esta ação não pode ser desfeita.`)) {
      return
    }
    setIdExcluindo(plataforma.id)
    setErroGeral('')
    setSucesso('')
    try {
      await plataformaApi.excluir(plataforma.id)
      setSucesso(`Plataforma "${plataforma.nome}" excluída.`)
      if (idEmEdicao === plataforma.id) {
        limparFormulario()
      }
      recarregar()
    } catch (falha) {
      // 102: a plataforma tem jogos e não pode ser excluída.
      setErroGeral(obterMensagemErro(falha))
    } finally {
      setIdExcluindo(null)
    }
  }

  const classeCampo = (nome) => `form-control ${errosCampos[nome] ? 'is-invalid' : ''}`

  return (
    <>
      <header className="mb-4">
        <p className="rg-eyebrow">Administração</p>
        <h1 className="rg-titulo-pagina">Plataformas</h1>
        <p className="rg-subtitulo">Cadastre os consoles usados no catálogo de jogos.</p>
      </header>

      {sucesso && <div className="alert alert-success" role="status">{sucesso}</div>}
      {erroGeral && <div className="alert alert-danger" role="alert">{erroGeral}</div>}

      <div className="row g-4">
        <div className="col-12 col-lg-4">
          <form className="rg-painel" onSubmit={aoSalvar} noValidate>
            <h2 className="h5 mb-3">{idEmEdicao ? 'Editar plataforma' : 'Nova plataforma'}</h2>

            <div className="mb-3">
              <label htmlFor="plataforma-nome" className="form-label">Nome</label>
              <input
                ref={campoNome}
                id="plataforma-nome"
                name="nome"
                className={classeCampo('nome')}
                value={formulario.nome}
                onChange={alterarCampo}
                maxLength={100}
                placeholder="Ex.: Super Nintendo"
                aria-describedby="plataforma-nome-erro"
              />
              <div id="plataforma-nome-erro" className="invalid-feedback">{errosCampos.nome}</div>
            </div>

            <div className="mb-3">
              <label htmlFor="plataforma-fabricante" className="form-label">Fabricante</label>
              <input
                id="plataforma-fabricante"
                name="fabricante"
                className={classeCampo('fabricante')}
                value={formulario.fabricante}
                onChange={alterarCampo}
                maxLength={100}
                placeholder="Ex.: Nintendo"
                aria-describedby="plataforma-fabricante-erro"
              />
              <div id="plataforma-fabricante-erro" className="invalid-feedback">{errosCampos.fabricante}</div>
            </div>

            <div className="mb-4">
              <label htmlFor="plataforma-ano" className="form-label">Ano de lançamento</label>
              <input
                id="plataforma-ano"
                name="anoLancamento"
                type="number"
                inputMode="numeric"
                className={classeCampo('anoLancamento')}
                value={formulario.anoLancamento}
                onChange={alterarCampo}
                placeholder="Ex.: 1990"
                aria-describedby="plataforma-ano-erro"
              />
              <div id="plataforma-ano-erro" className="invalid-feedback">{errosCampos.anoLancamento}</div>
            </div>

            <div className="d-flex flex-wrap gap-2">
              <button type="submit" className="btn btn-primary flex-grow-1" disabled={salvando}>
                {salvando ? 'Salvando...' : idEmEdicao ? 'Salvar alterações' : 'Cadastrar'}
              </button>
              {idEmEdicao && (
                <button type="button" className="btn btn-outline-light" onClick={limparFormulario} disabled={salvando}>
                  Cancelar
                </button>
              )}
            </div>
          </form>
        </div>

        <div className="col-12 col-lg-8">
          {carregando && <Carregando texto="Carregando plataformas..." />}
          {!carregando && erroLista && <MensagemErro mensagem={erroLista} aoTentarNovamente={recarregar} />}
          {!carregando && !erroLista && plataformas.length === 0 && (
            <EstadoVazio
              titulo="Nenhuma plataforma cadastrada"
              descricao="Use o formulário ao lado para cadastrar a primeira plataforma."
            />
          )}
          {!carregando && !erroLista && plataformas.length > 0 && (
            <div className="rg-painel p-0">
              <div className="table-responsive">
                <table className="table table-hover">
                  <caption className="visually-hidden">Plataformas cadastradas</caption>
                  <thead>
                    <tr>
                      <th scope="col">Nome</th>
                      <th scope="col">Fabricante</th>
                      <th scope="col">Ano</th>
                      <th scope="col" className="text-end">Ações</th>
                    </tr>
                  </thead>
                  <tbody>
                    {plataformas.map((plataforma) => (
                      <tr key={plataforma.id} className={idEmEdicao === plataforma.id ? 'table-active' : ''}>
                        <td className="fw-semibold">{plataforma.nome}</td>
                        <td>{plataforma.fabricante}</td>
                        <td>{plataforma.anoLancamento}</td>
                        <td className="text-end text-nowrap">
                          <button
                            type="button"
                            className="btn btn-outline-light btn-sm me-2"
                            onClick={() => editar(plataforma)}
                            aria-label={`Editar ${plataforma.nome}`}
                          >
                            Editar
                          </button>
                          <button
                            type="button"
                            className="btn btn-outline-danger btn-sm"
                            onClick={() => excluir(plataforma)}
                            disabled={idExcluindo === plataforma.id}
                            aria-label={`Excluir ${plataforma.nome}`}
                          >
                            {idExcluindo === plataforma.id ? 'Excluindo...' : 'Excluir'}
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </div>
      </div>
    </>
  )
}
