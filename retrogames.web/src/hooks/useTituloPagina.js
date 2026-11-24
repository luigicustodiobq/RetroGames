import { useEffect } from 'react'

/**
 * Atualiza o título da aba do navegador (Heurística #1 — visibilidade do status:
 * o usuário sabe em que página está, inclusive com várias abas abertas e leitores de tela).
 */
export function useTituloPagina(titulo) {
  useEffect(() => {
    document.title = titulo ? `${titulo} · RetroGames` : 'RetroGames — Loja de jogos retrô'
  }, [titulo])
}
