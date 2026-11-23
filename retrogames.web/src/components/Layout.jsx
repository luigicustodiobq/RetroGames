import { useState } from 'react'
import { Link, NavLink, Outlet } from 'react-router-dom'
import Logo from './Logo'

/**
 * Estrutura comum a todas as páginas: barra de navegação responsiva
 * (menu colapsável em telas estreitas), conteúdo da rota e rodapé.
 */
export default function Layout() {
  const [menuAberto, setMenuAberto] = useState(false)
  const fecharMenu = () => setMenuAberto(false)

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
            </ul>
          </div>
        </nav>
      </header>

      <main id="conteudo" className="container py-4" tabIndex={-1}>
        <Outlet />
      </main>

      <footer className="rg-rodape py-4">
        <div className="container d-flex flex-column flex-md-row gap-2 justify-content-between align-items-md-center">
          <span>© {new Date().getFullYear()} RetroGames — clássicos testados e garantidos.</span>
          <Link to="/">Voltar ao catálogo</Link>
        </div>
      </footer>
    </>
  )
}
