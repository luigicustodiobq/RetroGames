import { Navigate, Route, Routes } from 'react-router-dom'
import Layout from './components/Layout'
import RotaProtegida from './components/RotaProtegida'
import Catalogo from './pages/Catalogo/Catalogo'
import DetalheJogo from './pages/DetalheJogo/DetalheJogo'
import Login from './pages/Login/Login'
import NaoEncontrada from './pages/NaoEncontrada'

function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        {/* Rotas públicas */}
        <Route path="/" element={<Catalogo />} />
        <Route path="/jogos/:id" element={<DetalheJogo />} />
        <Route path="/login" element={<Login />} />

        {/* Administração: só para o perfil Administrador */}
        <Route path="/admin" element={<RotaProtegida somenteAdministrador />}>
          <Route index element={<Navigate to="/admin/jogos" replace />} />
        </Route>

        <Route path="*" element={<NaoEncontrada />} />
      </Route>
    </Routes>
  )
}

export default App
