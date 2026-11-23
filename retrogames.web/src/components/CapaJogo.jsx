// "Capa" gerada a partir do nome do jogo: sigla e um degradê com cor própria
// (o catálogo não tem imagens; cada jogo ganha uma identidade visual estável).

const CORES = [
  ['#3b1f5c', '#14111f'],
  ['#5c1f3b', '#16101a'],
  ['#1f4a5c', '#0f141a'],
  ['#5c3f1f', '#1a140f'],
  ['#1f5c45', '#0f1a15'],
  ['#2f2a6b', '#12101f'],
]

function sigla(nome) {
  const palavras = nome.replace(/[^\p{L}\p{N} ]/gu, '').split(' ').filter(Boolean)
  return palavras.slice(0, 2).map((palavra) => palavra[0]).join('').toUpperCase() || '?'
}

export default function CapaJogo({ nome, children }) {
  const indice = [...nome].reduce((soma, letra) => soma + letra.charCodeAt(0), 0) % CORES.length
  const [corA, corB] = CORES[indice]

  return (
    <div className="rg-capa" style={{ '--rg-capa-a': corA, '--rg-capa-b': corB }}>
      <span className="rg-capa-sigla" aria-hidden="true">{sigla(nome)}</span>
      {children}
    </div>
  )
}
