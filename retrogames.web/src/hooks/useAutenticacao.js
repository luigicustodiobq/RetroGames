import { useContext } from 'react'
import { AutenticacaoContext } from '../contexts/autenticacaoContext'

/** Acesso à sessão: { usuario, carregando, estaAutenticado, ehAdministrador, ehCliente, entrar, cadastrar, sair }. */
export function useAutenticacao() {
  const contexto = useContext(AutenticacaoContext)
  if (!contexto) {
    throw new Error('useAutenticacao deve ser usado dentro de <AutenticacaoProvider>.')
  }
  return contexto
}
