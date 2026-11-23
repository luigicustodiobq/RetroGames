import { Link } from 'react-router-dom'
import { formatarPreco } from '../lib/formato'
import CapaJogo from './CapaJogo'

/** Card de um jogo no catálogo. */
export default function CardJogo({ jogo }) {
  const esgotado = jogo.estoque === 0

  return (
    <article className="card rg-card-jogo">
      <CapaJogo nome={jogo.nome}>
        <span className="badge rg-badge-condicao rg-selo">{jogo.descricaoCondicao}</span>
        {esgotado && <span className="badge rg-badge-esgotado rg-selo rg-selo-direita">Esgotado</span>}
      </CapaJogo>

      <div className="card-body d-flex flex-column">
        <h3 className="card-title mb-1">{jogo.nome}</h3>
        <p className="rg-meta mb-2">
          {jogo.nomePlataforma} · {jogo.anoLancamento}
        </p>

        <div className="d-flex flex-wrap gap-1 mb-3">
          {jogo.generos.map((genero) => (
            <span key={genero} className="badge rg-badge-genero">{genero}</span>
          ))}
        </div>

        <div className="mt-auto d-flex align-items-center justify-content-between gap-2">
          <span className="rg-preco">{formatarPreco(jogo.preco)}</span>
          <Link
            to={`/jogos/${jogo.id}`}
            className="btn btn-outline-light btn-sm"
            aria-label={`Ver detalhes de ${jogo.nome}`}
          >
            Ver detalhes
          </Link>
        </div>
      </div>
    </article>
  )
}
