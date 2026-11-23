import { Route, Routes } from 'react-router-dom'
import Layout from './components/Layout'
import NaoEncontrada from './pages/NaoEncontrada'

function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<p className="text-body-secondary">Catálogo em construção.</p>} />
        <Route path="*" element={<NaoEncontrada />} />
      </Route>
    </Routes>
  )
}

export default App
