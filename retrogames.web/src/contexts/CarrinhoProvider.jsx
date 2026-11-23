import { useMemo, useState } from 'react'
import { CarrinhoContext } from './carrinhoContext'

const CHAVE_ARMAZENAMENTO = 'retrogames_carrinho'

function lerItensSalvos() {
  try {
    const salvo = JSON.parse(localStorage.getItem(CHAVE_ARMAZENAMENTO) ?? '[]')
    return Array.isArray(salvo) ? salvo : []
  } catch {
    return []
  }
}

/** Carrinho de compras (C02: expõe a quantidade total de itens para o menu). */
export default function CarrinhoProvider({ children }) {
  const [itens] = useState(lerItensSalvos)

  const valor = useMemo(
    () => ({
      itens,
      totalItens: itens.reduce((soma, item) => soma + item.quantidade, 0),
    }),
    [itens],
  )

  return <CarrinhoContext.Provider value={valor}>{children}</CarrinhoContext.Provider>
}
