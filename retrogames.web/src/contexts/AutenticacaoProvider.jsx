import { useCallback, useEffect, useMemo, useState } from 'react'
import { autenticacaoApi } from '../api/servicos'
import { AutenticacaoContext } from './autenticacaoContext'

const PERFIL_ADMINISTRADOR = 'Administrador'

/**
 * Guarda na memória quem está logado. A sessão em si é o cookie HttpOnly da API:
 * nada é salvo em localStorage/sessionStorage. Ao abrir o site, pergunta à API
 * quem é o usuário (GET /Autenticacao/eu); 401 ali significa visitante.
 */
export default function AutenticacaoProvider({ children }) {
  const [usuario, setUsuario] = useState(null)
  const [carregando, setCarregando] = useState(true)

  useEffect(() => {
    let ativo = true
    autenticacaoApi
      .usuarioAtual()
      .then((dados) => ativo && setUsuario(dados))
      .catch(() => ativo && setUsuario(null))
      .finally(() => ativo && setCarregando(false))

    // Disparado pelo interceptor do Axios quando uma rota protegida responde 401.
    const aoExpirarSessao = () => setUsuario(null)
    window.addEventListener('sessao:expirada', aoExpirarSessao)

    return () => {
      ativo = false
      window.removeEventListener('sessao:expirada', aoExpirarSessao)
    }
  }, [])

  const entrar = useCallback(async (email, senha) => {
    const dados = await autenticacaoApi.entrar(email, senha)
    setUsuario(dados)
    return dados
  }, [])

  // C01: o cadastro já devolve o usuário com a sessão iniciada pela API.
  const cadastrar = useCallback(async (nome, email, senha) => {
    const dados = await autenticacaoApi.cadastrar(nome, email, senha)
    setUsuario(dados)
    return dados
  }, [])

  const sair = useCallback(async () => {
    try {
      await autenticacaoApi.sair()
    } finally {
      setUsuario(null)
    }
  }, [])

  const valor = useMemo(
    () => ({
      usuario,
      carregando,
      estaAutenticado: usuario !== null,
      ehAdministrador: usuario?.perfil === PERFIL_ADMINISTRADOR,
      // C02: cliente = logado e não administrador.
      ehCliente: usuario !== null && usuario.perfil !== PERFIL_ADMINISTRADOR,
      entrar,
      cadastrar,
      sair,
    }),
    [usuario, carregando, entrar, cadastrar, sair],
  )

  return <AutenticacaoContext.Provider value={valor}>{children}</AutenticacaoContext.Provider>
}
