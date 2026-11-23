import { Link } from 'react-router-dom'

/** Página 404: rota inexistente. */
export default function NaoEncontrada() {
  return (
    <section className="rg-painel rg-estado mx-auto" style={{ maxWidth: 560 }}>
      <div className="rg-estado-icone" aria-hidden="true">👾</div>
      <p className="rg-eyebrow">Erro 404</p>
      <h1 className="h2 mb-2">Game over para esta página</h1>
      <p className="text-body-secondary mb-4">
        O endereço que você tentou abrir não existe ou foi movido.
        Que tal voltar e escolher um clássico?
      </p>
      <Link to="/" className="btn btn-primary">Voltar ao catálogo</Link>
    </section>
  )
}
