import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { obterMensagemErro } from '../../api/erros'
import { generoApi, jogoApi, plataformaApi } from '../../api/servicos'
import CardJogo from '../../components/CardJogo'
import { Carregando, EstadoVazio, MensagemErro } from '../../components/MensagensEstado'
import Paginacao from '../../components/Paginacao'
import { useTituloPagina } from '../../hooks/useTituloPagina'

const TAMANHO_PAGINA = 12

/**
 * Catálogo (rota "/"). Os filtros ficam na URL (?busca=&idPlataforma=&idGenero=&pagina=),
 * então a busca pode ser compartilhada, recarregada e navegada com o botão Voltar.
 */
export default function Catalogo() {
  useTituloPagina('Catálogo')
  const [searchParams, setSearchParams] = useSearchParams()
  const busca = searchParams.get('busca') ?? ''
  const idPlataforma = searchParams.get('idPlataforma') ?? ''
  const idGenero = searchParams.get('idGenero') ?? ''
  const pagina = Math.max(1, Number(searchParams.get('pagina')) || 1)

  const [plataformas, setPlataformas] = useState([])
  const [generos, setGeneros] = useState([])
  const [resultado, setResultado] = useState(null)
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState('')
  const [tentativa, setTentativa] = useState(0)

  // Opções dos filtros: carregadas uma vez.
  useEffect(() => {
    Promise.all([plataformaApi.listar(), generoApi.listar()])
      .then(([listaPlataformas, listaGeneros]) => {
        setPlataformas(listaPlataformas)
        setGeneros(listaGeneros)
      })
      .catch(() => {
        // Sem as opções, o catálogo continua funcionando com a busca por nome.
      })
  }, [])

  // Jogos: recarregados sempre que um filtro da URL muda.
  useEffect(() => {
    let ativo = true
    setCarregando(true)
    setErro('')

    jogoApi
      .listar({
        busca: busca || undefined,
        idPlataforma: idPlataforma || undefined,
        idGenero: idGenero || undefined,
        pagina,
        tamanhoPagina: TAMANHO_PAGINA,
      })
      .then((dados) => ativo && setResultado(dados))
      .catch((falha) => ativo && setErro(obterMensagemErro(falha)))
      .finally(() => ativo && setCarregando(false))

    return () => {
      ativo = false
    }
  }, [busca, idPlataforma, idGenero, pagina, tentativa])

  /** Altera um filtro e volta para a página 1. */
  function alterarFiltro(nome, valor) {
    const novos = new URLSearchParams(searchParams)
    if (valor) {
      novos.set(nome, valor)
    } else {
      novos.delete(nome)
    }
    novos.delete('pagina')
    setSearchParams(novos)
  }

  function aoBuscar(evento) {
    evento.preventDefault()
    const texto = new FormData(evento.currentTarget).get('busca')?.toString().trim() ?? ''
    alterarFiltro('busca', texto)
  }

  function mudarPagina(novaPagina) {
    const novos = new URLSearchParams(searchParams)
    novos.set('pagina', String(novaPagina))
    setSearchParams(novos)
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  const limparFiltros = () => setSearchParams({})
  const temFiltro = Boolean(busca || idPlataforma || idGenero)

  return (
    <>
      <section className="rg-hero mb-4">
        <p className="rg-eyebrow">Cartuchos, discos e nostalgia</p>
        <h1 className="mb-3">Clássicos que nunca saem de moda.</h1>
        <p className="mb-0">
          Jogos originais de Super Nintendo, Mega Drive, PlayStation e muito mais —
          revisados, testados e prontos para mais uma partida.
        </p>
      </section>

      <form className="rg-painel mb-4" role="search" onSubmit={aoBuscar} aria-label="Filtros do catálogo">
        <div className="row g-3 align-items-end">
          <div className="col-12 col-lg-5">
            <label htmlFor="filtro-busca" className="form-label">Buscar por nome</label>
            <div className="input-group">
              <input
                key={busca}
                id="filtro-busca"
                name="busca"
                type="search"
                className="form-control"
                placeholder="Ex.: Mario, Sonic, Zelda..."
                defaultValue={busca}
              />
              <button type="submit" className="btn btn-primary">Buscar</button>
            </div>
          </div>
          <div className="col-12 col-md-6 col-lg-3">
            <label htmlFor="filtro-plataforma" className="form-label">Plataforma</label>
            <select
              id="filtro-plataforma"
              className="form-select"
              value={idPlataforma}
              onChange={(e) => alterarFiltro('idPlataforma', e.target.value)}
            >
              <option value="">Todas</option>
              {plataformas.map((plataforma) => (
                <option key={plataforma.id} value={plataforma.id}>{plataforma.nome}</option>
              ))}
            </select>
          </div>
          <div className="col-12 col-md-6 col-lg-2">
            <label htmlFor="filtro-genero" className="form-label">Gênero</label>
            <select
              id="filtro-genero"
              className="form-select"
              value={idGenero}
              onChange={(e) => alterarFiltro('idGenero', e.target.value)}
            >
              <option value="">Todos</option>
              {generos.map((genero) => (
                <option key={genero.id} value={genero.id}>{genero.nome}</option>
              ))}
            </select>
          </div>
          <div className="col-12 col-lg-2 d-grid">
            <button type="button" className="btn btn-outline-light" onClick={limparFiltros} disabled={!temFiltro}>
              Limpar filtros
            </button>
          </div>
        </div>
      </form>

      {carregando && <Carregando texto="Carregando jogos..." />}

      {!carregando && erro && (
        <MensagemErro mensagem={erro} aoTentarNovamente={() => setTentativa((n) => n + 1)} />
      )}

      {!carregando && !erro && resultado && resultado.itens.length === 0 && (
        <EstadoVazio
          titulo="Nenhum jogo encontrado"
          descricao={temFiltro
            ? 'Nenhum jogo atende aos filtros escolhidos. Tente outro nome, plataforma ou gênero.'
            : 'O catálogo ainda está vazio.'}
          textoAcao={temFiltro ? 'Limpar filtros' : undefined}
          aoClicar={limparFiltros}
        />
      )}

      {!carregando && !erro && resultado && resultado.itens.length > 0 && (
        <>
          <p className="text-body-secondary mb-3" aria-live="polite">
            {resultado.totalItens} {resultado.totalItens === 1 ? 'jogo encontrado' : 'jogos encontrados'}
            {resultado.totalPaginas > 1 && ` · página ${resultado.pagina} de ${resultado.totalPaginas}`}
          </p>
          <div className="row row-cols-1 row-cols-md-2 row-cols-lg-4 g-4">
            {resultado.itens.map((jogo) => (
              <div key={jogo.id} className="col">
                <CardJogo jogo={jogo} />
              </div>
            ))}
          </div>
          <Paginacao
            paginaAtual={resultado.pagina}
            totalPaginas={resultado.totalPaginas}
            aoMudarPagina={mudarPagina}
          />
        </>
      )}
    </>
  )
}
