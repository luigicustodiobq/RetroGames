/** Navegação entre páginas. Não aparece quando há uma página só. */
export default function Paginacao({ paginaAtual, totalPaginas, aoMudarPagina }) {
  if (totalPaginas <= 1) {
    return null
  }

  const paginas = Array.from({ length: totalPaginas }, (_, indice) => indice + 1)

  return (
    <nav aria-label="Paginação" className="mt-4">
      <ul className="pagination justify-content-center flex-wrap">
        <li className={`page-item ${paginaAtual <= 1 ? 'disabled' : ''}`}>
          <button
            type="button"
            className="page-link"
            onClick={() => aoMudarPagina(paginaAtual - 1)}
            disabled={paginaAtual <= 1}
            aria-label="Página anterior"
          >
            ‹
          </button>
        </li>
        {paginas.map((pagina) => (
          <li key={pagina} className={`page-item ${pagina === paginaAtual ? 'active' : ''}`}>
            <button
              type="button"
              className="page-link"
              onClick={() => aoMudarPagina(pagina)}
              aria-current={pagina === paginaAtual ? 'page' : undefined}
              aria-label={`Página ${pagina}`}
            >
              {pagina}
            </button>
          </li>
        ))}
        <li className={`page-item ${paginaAtual >= totalPaginas ? 'disabled' : ''}`}>
          <button
            type="button"
            className="page-link"
            onClick={() => aoMudarPagina(paginaAtual + 1)}
            disabled={paginaAtual >= totalPaginas}
            aria-label="Próxima página"
          >
            ›
          </button>
        </li>
      </ul>
    </nav>
  )
}
