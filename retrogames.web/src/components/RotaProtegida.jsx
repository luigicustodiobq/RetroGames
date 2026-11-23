import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAutenticacao } from '../hooks/useAutenticacao'
import { Carregando } from './MensagensEstado'

/**
 * Protege um grupo de rotas no front-end. Visitante vai para o login (e volta depois);
 * cliente tentando abrir rota de administrador vai para o catálogo.
 * A proteção de verdade está na API: aqui só evitamos telas que dariam 401/403.
 */
export default function RotaProtegida({ somenteAdministrador = false }) {
  const { carregando, estaAutenticado, ehAdministrador } = useAutenticacao()
  const location = useLocation()

  if (carregando) {
    return <Carregando texto="Verificando sessão..." />
  }

  if (!estaAutenticado) {
    const voltarPara = encodeURIComponent(location.pathname + location.search)
    return <Navigate to={`/login?voltarPara=${voltarPara}`} replace />
  }

  if (somenteAdministrador && !ehAdministrador) {
    return <Navigate to="/" replace />
  }

  return <Outlet />
}
