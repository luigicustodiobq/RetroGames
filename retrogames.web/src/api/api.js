import axios from 'axios'

// Única instância do Axios do front-end: nenhum outro arquivo importa 'axios'.
// baseURL '/api' passa pelo proxy do Vite até a API (porta 5080).
// withCredentials envia o cookie de sessão (retrogames.sessao) em toda requisição.
const api = axios.create({
  baseURL: '/api',
  withCredentials: true,
})

export default api
