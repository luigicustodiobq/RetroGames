import { Route, Routes } from 'react-router-dom'
import Layout from './components/Layout'
import Catalogo from './pages/Catalogo/Catalogo'
import NaoEncontrada from './pages/NaoEncontrada'

function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<Catalogo />} />
        <Route path="*" element={<NaoEncontrada />} />
      </Route>
    </Routes>
  )
}

export default App
