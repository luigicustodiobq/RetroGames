import { useEffect, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { obterMensagemErro } from '../../api/erros'
import { jogoApi } from '../../api/servicos'
import { Carregando, EstadoVazio, MensagemErro } from '../../components/MensagensEstado'
import Paginacao from '../../components/Paginacao'
import { formatarPreco } from '../../lib/formato'
import { useTituloPagina } from '../../hooks/useTituloPagina'

const TAMANHO_PAGINA = 10

/** Administração de jogos (rota "/admin/jogos"): lista ativos e inativos. */
export default function Jogos() {
  useTituloPagina('Jogos · Administração')
  const [searchParams, setSearchParams] = useSearchParams()
  const busca = searchParams.get('busca') ?? ''
  const pagina = Math.max(1, Number(searchParams.get('pagina')) || 1)

  const [resultado, setResultado] = useState(null)
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState('')
  const [tentativa, setTentativa] = useState(0)
  const [aviso, setAviso] = useState({ tipo: '', texto: '' })
  const [idDesativando, setIdDesativando] = useState(null)

  useEffect(() => {
    let ativo = true
    setCarregando(true)
    setErro('')
    jogoApi
      .listar({ busca: busca || undefined, pagina, tamanhoPagina: TAMANHO_PAGINA, incluirInativos: true })
      .then((dados) => ativo && setResultado(dados))
      .catch((falha) => ativo && setErro(obterMensagemErro(falha)))
      .finally(() => ativo && setCarregando(false))
    return () => {
      ativo = false
    }
  }, [busca, pagina, tentativa])

  function aoBuscar(evento) {
    evento.preventDefault()
    const texto = new FormData(evento.currentTarget).get('busca')?.toString().trim() ?? ''
    setSearchParams(texto ? { busca: texto } : {})
  }

  function mudarPagina(novaPagina) {
    const novos = new URLSearchParams(searchParams)
    novos.set('pagina', String(novaPagina))
    setSearchParams(novos)
  }

  async function desativar(jogo) {
    if (!window.confirm(`Desativar "${jogo.nome}"? Ele deixará de aparecer no catálogo.`)) {
      return
    }
    setIdDesativando(jogo.id)
    setAviso({ tipo: '', texto: '' })
    try {
      await jogoApi.desativar(jogo.id)
      setAviso({ tipo: 'success', texto: `"${jogo.nome}" foi desativado e saiu do catálogo.` })
      setTentativa((n) => n + 1)
    } catch (falha) {
      setAviso({ tipo: 'danger', texto: obterMensagemErro(falha) })
    } finally {
      setIdDesativando(null)
    }
  }

  return (
    <>
      <header className="d-flex flex-column flex-md-row justify-content-between align-items-md-end gap-3 mb-4">
        <div>
          <p className="rg-eyebrow">Administração</p>
          <h1 className="rg-titulo-pagina">Jogos</h1>
          <p className="rg-subtitulo">Inclua, edite e desative os jogos do catálogo.</p>
        </div>
        <Link to="/admin/jogos/novo" className="btn btn-primary">+ Novo jogo</Link>
      </header>

      {aviso.texto && (
        <div className={`alert alert-${aviso.tipo}`} role={aviso.tipo === 'danger' ? 'alert' : 'status'}>
          {aviso.texto}
        </div>
      )}

      <form className="rg-painel mb-4" role="search" onSubmit={aoBuscar}>
        <label htmlFor="admin-busca" className="form-label">Buscar por nome</label>
        <div className="input-group">
          <input
            key={busca}
            id="admin-busca"
            name="busca"
            type="search"
            className="form-control"
            defaultValue={busca}
            placeholder="Nome do jogo"
          />
          <button type="submit" className="btn btn-primary">Buscar</button>
        </div>
      </form>

      {carregando && <Carregando texto="Carregando jogos..." />}
      {!carregando && erro && <MensagemErro mensagem={erro} aoTentarNovamente={() => setTentativa((n) => n + 1)} />}
      {!carregando && !erro && resultado?.itens.length === 0 && (
        <EstadoVazio
          titulo="Nenhum jogo encontrado"
          descricao={busca ? 'Nenhum jogo com esse nome.' : 'Cadastre o primeiro jogo do catálogo.'}
          textoAcao={busca ? 'Limpar busca' : 'Novo jogo'}
          para={busca ? '/admin/jogos' : '/admin/jogos/novo'}
        />
      )}
      {!carregando && !erro && resultado?.itens.length > 0 && (
        <>
          <div className="rg-painel p-0">
            <div className="table-responsive">
              <table className="table table-hover">
                <caption className="visually-hidden">Jogos cadastrados</caption>
                <thead>
                  <tr>
                    <th scope="col">Jogo</th>
                    <th scope="col">Plataforma</th>
                    <th scope="col" className="text-end">Preço</th>
                    <th scope="col" className="text-end">Estoque</th>
                    <th scope="col">Situação</th>
                    <th scope="col" className="text-end">Ações</th>
                  </tr>
                </thead>
                <tbody>
                  {resultado.itens.map((jogo) => (
                    <tr key={jogo.id}>
                      <td>
                        <span className="fw-semibold d-block">{jogo.nome}</span>
                        <span className="rg-meta">{jogo.descricaoCondicao} · {jogo.anoLancamento}</span>
                      </td>
                      <td>{jogo.nomePlataforma}</td>
                      <td className="text-end text-nowrap">{formatarPreco(jogo.preco)}</td>
                      <td className="text-end">{jogo.estoque}</td>
                      <td>
                        <span className={`badge ${jogo.ativo ? 'rg-badge-ativo' : 'rg-badge-inativo'}`}>
                          {jogo.ativo ? 'Ativo' : 'Inativo'}
                        </span>
                      </td>
                      <td className="text-end text-nowrap">
                        <Link
                          to={`/admin/jogos/${jogo.id}/editar`}
                          className="btn btn-outline-light btn-sm me-2"
                          aria-label={`Editar ${jogo.nome}`}
                        >
                          Editar
                        </Link>
                        <button
                          type="button"
                          className="btn btn-outline-danger btn-sm"
                          onClick={() => desativar(jogo)}
                          disabled={!jogo.ativo || idDesativando === jogo.id}
                          aria-label={`Desativar ${jogo.nome}`}
                          title={jogo.ativo ? undefined : 'Este jogo já está inativo'}
                        >
                          {idDesativando === jogo.id ? 'Desativando...' : 'Desativar'}
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
          <Paginacao paginaAtual={resultado.pagina} totalPaginas={resultado.totalPaginas} aoMudarPagina={mudarPagina} />
        </>
      )}
    </>
  )
}
