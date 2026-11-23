import { useState } from 'react'
import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAutenticacao } from '../hooks/useAutenticacao'
import Logo from './Logo'

const ANO_ATUAL = new Date().getFullYear()

/**
 * Estrutura comum a todas as páginas: barra de navegação responsiva
 * (menu colapsável em telas estreitas), conteúdo da rota e rodapé.
 * O menu muda conforme quem está logado (visitante, cliente ou administrador).
 */
export default function Layout() {
  const { usuario, estaAutenticado, ehAdministrador, sair } = useAutenticacao()
  const navigate = useNavigate()
  const [menuAberto, setMenuAberto] = useState(false)
  const fecharMenu = () => setMenuAberto(false)

  async function aoSair() {
    fecharMenu()
    await sair()
    navigate('/')
  }

  return (
    <>
      <a href="#conteudo" className="rg-pular-conteudo">Pular para o conteúdo</a>

      <header className="rg-navbar sticky-top">
        <nav className="navbar navbar-expand-lg container" aria-label="Navegação principal">
          <Link to="/" className="navbar-brand" onClick={fecharMenu}>
            <Logo />
            <span>Retro<span className="brand-destaque">Games</span></span>
          </Link>

          <button
            type="button"
            className="navbar-toggler"
            aria-controls="menu-principal"
            aria-expanded={menuAberto}
            aria-label={menuAberto ? 'Fechar menu' : 'Abrir menu'}
            onClick={() => setMenuAberto((aberto) => !aberto)}
          >
            <span className="navbar-toggler-icon" />
          </button>

          <div id="menu-principal" className={`collapse navbar-collapse ${menuAberto ? 'show' : ''}`}>
            <ul className="navbar-nav me-auto gap-lg-1">
              <li className="nav-item">
                <NavLink to="/" end className="nav-link" onClick={fecharMenu}>
                  Catálogo
                </NavLink>
              </li>

              {ehAdministrador && (
                <li className="nav-item dropdown">
                  <button
                    type="button"
                    className="nav-link dropdown-toggle btn btn-link text-start"
                    data-bs-toggle="dropdown"
                    aria-expanded="false"
                  >
                    Administração
                  </button>
                  <ul className="dropdown-menu">
                    <li>
                      <Link to="/admin/jogos" className="dropdown-item" onClick={fecharMenu}>Jogos</Link>
                    </li>
                    <li>
                      <Link to="/admin/plataformas" className="dropdown-item" onClick={fecharMenu}>Plataformas</Link>
                    </li>
                  </ul>
                </li>
              )}
            </ul>

            <ul className="navbar-nav align-items-lg-center gap-2">
              {estaAutenticado ? (
                <>
                  <li className="nav-item rg-saudacao px-lg-2">
                    Olá, <strong>{usuario.nome}</strong>
                  </li>
                  <li className="nav-item">
                    <button type="button" className="btn btn-outline-light btn-sm" onClick={aoSair}>
                      Sair
                    </button>
                  </li>
                </>
              ) : (
                <>
                  <li className="nav-item">
                    <NavLink to="/login" className="nav-link" onClick={fecharMenu}>Entrar</NavLink>
                  </li>
                  <li className="nav-item">
                    <Link to="/cadastro" className="btn btn-primary btn-sm" onClick={fecharMenu}>Cadastrar</Link>
                  </li>
                </>
              )}
            </ul>
          </div>
        </nav>
      </header>

      <main id="conteudo" className="container py-4" tabIndex={-1}>
        <Outlet />
      </main>

      <footer className="rg-rodape py-4">
        <div className="container d-flex flex-column flex-md-row gap-2 justify-content-between align-items-md-center">
          <span>© {ANO_ATUAL} RetroGames — clássicos testados e garantidos.</span>
          <Link to="/">Voltar ao catálogo</Link>
        </div>
      </footer>
    </>
  )
}
