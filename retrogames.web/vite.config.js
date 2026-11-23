import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
// O proxy encaminha /api para a API .NET (porta 5080). Assim o navegador enxerga
// front-end e API na mesma origem e o cookie de sessão (SameSite=Strict) é enviado.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': 'http://localhost:5080',
    },
  },
})
