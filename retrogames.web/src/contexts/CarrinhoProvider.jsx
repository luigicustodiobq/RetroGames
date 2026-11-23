import { useCallback, useEffect, useMemo, useState } from 'react'
import { CarrinhoContext } from './carrinhoContext'

// Só os itens do carrinho ficam no navegador. A sessão continua no cookie HttpOnly
// da API: nenhum token ou dado de login é salvo aqui (RS01).
const CHAVE_ARMAZENAMENTO = 'retrogames_carrinho'
const QUANTIDADE_MAXIMA_POR_ITEM = 10

function lerItensSalvos() {
  try {
    const salvo = JSON.parse(localStorage.getItem(CHAVE_ARMAZENAMENTO) ?? '[]')
    return Array.isArray(salvo) ? salvo.filter((item) => item && item.idJogo && item.quantidade > 0) : []
  } catch {
    return []
  }
}

/** Maior quantidade permitida para um item: até 10 e nunca acima do estoque. */
function limiteDoItem(estoque) {
  return Math.max(0, Math.min(QUANTIDADE_MAXIMA_POR_ITEM, Number(estoque) || 0))
}

function limitar(quantidade, estoque) {
  const limite = limiteDoItem(estoque)
  return Math.min(Math.max(1, Math.floor(Number(quantidade) || 1)), limite)
}

/**
 * Carrinho de compras persistido no localStorage (chave retrogames_carrinho).
 * O preço guardado serve só para exibição: na finalização, a API usa o preço do banco.
 */
export default function CarrinhoProvider({ children }) {
  const [itens, setItens] = useState(lerItensSalvos)

  // Sincroniza o estado com o localStorage (sistema externo) a cada mudança.
  useEffect(() => {
    localStorage.setItem(CHAVE_ARMAZENAMENTO, JSON.stringify(itens))
  }, [itens])

  /** Adiciona (ou soma ao item existente) respeitando o limite do jogo. */
  const adicionarItem = useCallback((jogo, quantidade = 1) => {
    if (limiteDoItem(jogo.estoque) === 0) {
      return
    }
    setItens((atuais) => {
      const existente = atuais.find((item) => item.idJogo === jogo.id)
      const item = {
        idJogo: jogo.id,
        nome: jogo.nome,
        nomePlataforma: jogo.nomePlataforma,
        preco: jogo.preco,
        estoque: jogo.estoque,
        quantidade: limitar((existente?.quantidade ?? 0) + quantidade, jogo.estoque),
      }
      return existente
        ? atuais.map((atual) => (atual.idJogo === jogo.id ? item : atual))
        : [...atuais, item]
    })
  }, [])

  /** Quantidade limitada entre 1 e Math.min(10, estoque do jogo). */
  const alterarQuantidade = useCallback((idJogo, quantidade) => {
    setItens((atuais) =>
      atuais.map((item) =>
        item.idJogo === idJogo ? { ...item, quantidade: limitar(quantidade, item.estoque) } : item,
      ),
    )
  }, [])

  const removerItem = useCallback((idJogo) => {
    setItens((atuais) => atuais.filter((item) => item.idJogo !== idJogo))
  }, [])

  const limparCarrinho = useCallback(() => setItens([]), [])

  const valor = useMemo(
    () => ({
      itens,
      totalItens: itens.reduce((soma, item) => soma + item.quantidade, 0),
      valorTotal: itens.reduce((soma, item) => soma + item.quantidade * Number(item.preco), 0),
      limiteDoItem,
      adicionarItem,
      alterarQuantidade,
      removerItem,
      limparCarrinho,
    }),
    [itens, adicionarItem, alterarQuantidade, removerItem, limparCarrinho],
  )

  return <CarrinhoContext.Provider value={valor}>{children}</CarrinhoContext.Provider>
}
