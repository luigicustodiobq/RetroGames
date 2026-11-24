import { useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { obterMensagemErro } from '../../api/erros'
import { useAutenticacao } from '../../hooks/useAutenticacao'
import { useTituloPagina } from '../../hooks/useTituloPagina'

/** Tela de login (rota "/login"). Após entrar, volta para ?voltarPara= ou para o catálogo. */
export default function Login() {
  useTituloPagina('Entrar')
  const { entrar } = useAutenticacao()
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const voltarPara = searchParams.get('voltarPara') || '/'

  const [email, setEmail] = useState('')
  const [senha, setSenha] = useState('')
  const [enviando, setEnviando] = useState(false)
  const [erro, setErro] = useState('')

  async function aoEnviar(evento) {
    evento.preventDefault()
    setEnviando(true)
    setErro('')
    try {
      await entrar(email.trim(), senha)
      navigate(voltarPara, { replace: true })
    } catch (falha) {
      setErro(obterMensagemErro(falha))
      setEnviando(false)
    }
  }

  return (
    <section className="mx-auto" style={{ maxWidth: 440 }}>
      <div className="rg-painel">
        <p className="rg-eyebrow">Bem-vindo de volta</p>
        <h1 className="rg-titulo-pagina mb-1">Entrar</h1>
        <p className="rg-subtitulo mb-4">Acesse sua conta para comprar e acompanhar seus pedidos.</p>

        {erro && (
          <div className="alert alert-danger" role="alert">{erro}</div>
        )}

        <form onSubmit={aoEnviar} noValidate={false}>
          <div className="mb-3">
            <label htmlFor="login-email" className="form-label">E-mail</label>
            <input
              id="login-email"
              type="email"
              className="form-control"
              autoComplete="email"
              placeholder="voce@exemplo.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              autoFocus
            />
          </div>
          <div className="mb-4">
            <label htmlFor="login-senha" className="form-label">Senha</label>
            <input
              id="login-senha"
              type="password"
              className="form-control"
              autoComplete="current-password"
              value={senha}
              onChange={(e) => setSenha(e.target.value)}
              required
            />
          </div>
          <button type="submit" className="btn btn-primary w-100" disabled={enviando}>
            {enviando && <span className="spinner-border spinner-border-sm" aria-hidden="true" />}
            {enviando ? 'Entrando...' : 'Entrar'}
          </button>
        </form>

        <p className="text-center text-body-secondary mt-4 mb-0">
          Ainda não tem conta?{' '}
          <Link to={`/cadastro${voltarPara !== '/' ? `?voltarPara=${encodeURIComponent(voltarPara)}` : ''}`}>
            Cadastre-se
          </Link>
        </p>
      </div>
    </section>
  )
}
