import { useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { obterErrosDosCampos, obterMensagemErro } from '../../api/erros'
import { useAutenticacao } from '../../hooks/useAutenticacao'
import { useTituloPagina } from '../../hooks/useTituloPagina'

const SENHA_MINIMA = 6

/** C01: cadastro de cliente (rota "/cadastro"). Ao concluir, o cliente já fica logado. */
export default function Cadastro() {
  useTituloPagina('Criar conta')
  const { cadastrar } = useAutenticacao()
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const voltarPara = searchParams.get('voltarPara') || '/'

  const [formulario, setFormulario] = useState({ nome: '', email: '', senha: '' })
  const [errosCampos, setErrosCampos] = useState({})
  const [erroGeral, setErroGeral] = useState('')
  const [enviando, setEnviando] = useState(false)

  function alterarCampo(evento) {
    const { name, value } = evento.target
    setFormulario((atual) => ({ ...atual, [name]: value }))
    setErrosCampos((atuais) => ({ ...atuais, [name]: undefined }))
  }

  async function aoEnviar(evento) {
    evento.preventDefault()
    setEnviando(true)
    setErrosCampos({})
    setErroGeral('')
    try {
      await cadastrar(formulario.nome.trim(), formulario.email.trim(), formulario.senha)
      navigate(voltarPara, { replace: true })
    } catch (falha) {
      setErrosCampos(obterErrosDosCampos(falha))
      setErroGeral(obterMensagemErro(falha))
      setEnviando(false)
    }
  }

  const tamanhoSenha = formulario.senha.length
  const senhaOk = tamanhoSenha >= SENHA_MINIMA
  const classeCampo = (nome) => `form-control ${errosCampos[nome] ? 'is-invalid' : ''}`

  return (
    <section className="mx-auto" style={{ maxWidth: 480 }}>
      <div className="rg-painel">
        <p className="rg-eyebrow">Nova conta</p>
        <h1 className="rg-titulo-pagina mb-1">Criar conta</h1>
        <p className="rg-subtitulo mb-4">Leva menos de um minuto. Depois é só escolher seus clássicos.</p>

        {erroGeral && <div className="alert alert-danger" role="alert">{erroGeral}</div>}

        <form onSubmit={aoEnviar} noValidate>
          <div className="mb-3">
            <label htmlFor="cadastro-nome" className="form-label">Nome completo</label>
            <input
              id="cadastro-nome"
              name="nome"
              className={classeCampo('nome')}
              autoComplete="name"
              maxLength={150}
              value={formulario.nome}
              onChange={alterarCampo}
              aria-describedby="cadastro-nome-erro"
              autoFocus
            />
            <div id="cadastro-nome-erro" className="invalid-feedback">{errosCampos.nome}</div>
          </div>

          <div className="mb-3">
            <label htmlFor="cadastro-email" className="form-label">E-mail</label>
            <input
              id="cadastro-email"
              name="email"
              type="email"
              className={classeCampo('email')}
              autoComplete="email"
              maxLength={150}
              placeholder="voce@exemplo.com"
              value={formulario.email}
              onChange={alterarCampo}
              aria-describedby="cadastro-email-erro"
            />
            <div id="cadastro-email-erro" className="invalid-feedback">{errosCampos.email}</div>
          </div>

          <div className="mb-4">
            <label htmlFor="cadastro-senha" className="form-label">Senha</label>
            <input
              id="cadastro-senha"
              name="senha"
              type="password"
              className={classeCampo('senha')}
              autoComplete="new-password"
              value={formulario.senha}
              onChange={alterarCampo}
              aria-describedby="cadastro-senha-regra cadastro-senha-erro"
            />
            {/* Heurística #10 (ajuda e documentação): a regra da senha aparece antes do erro. */}
            <div id="cadastro-senha-regra" className={`form-text ${senhaOk ? 'text-success' : ''}`} aria-live="polite">
              {senhaOk ? '✓ ' : ''}Use no mínimo {SENHA_MINIMA} caracteres
              {tamanhoSenha > 0 && !senhaOk && ` (faltam ${SENHA_MINIMA - tamanhoSenha})`}.
            </div>
            <div id="cadastro-senha-erro" className="invalid-feedback">{errosCampos.senha}</div>
          </div>

          <button type="submit" className="btn btn-primary w-100" disabled={enviando}>
            {enviando && <span className="spinner-border spinner-border-sm" aria-hidden="true" />}
            {enviando ? 'Criando conta...' : 'Criar conta'}
          </button>
        </form>

        <p className="text-center text-body-secondary mt-4 mb-0">
          Já tem conta?{' '}
          <Link to={`/login${voltarPara !== '/' ? `?voltarPara=${encodeURIComponent(voltarPara)}` : ''}`}>Entrar</Link>
        </p>
      </div>
    </section>
  )
}
