import axios from 'axios'

// Única instância do Axios do front-end: nenhum outro arquivo importa 'axios'.
// baseURL '/api' passa pelo proxy do Vite até a API (porta 5080).
// withCredentials envia o cookie de sessão (retrogames.sessao) em toda requisição.
const api = axios.create({
  baseURL: '/api',
  withCredentials: true,
})

// Rotas em que o 401 é uma resposta esperada (não significa "sessão expirou"):
// /Autenticacao/eu responde 401 para visitante e /Autenticacao/login para senha errada.
const ROTAS_SEM_AVISO_DE_SESSAO = ['/Autenticacao/eu', '/Autenticacao/login']

api.interceptors.response.use(
  (resposta) => resposta,
  (error) => {
    const url = error.config?.url ?? ''
    if (error.response?.status === 401 && !ROTAS_SEM_AVISO_DE_SESSAO.some((rota) => url.endsWith(rota))) {
      // O AutenticacaoProvider ouve este evento e limpa o usuário da tela.
      window.dispatchEvent(new Event('sessao:expirada'))
    }
    return Promise.reject(error)
  },
)

export default api
