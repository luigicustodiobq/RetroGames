import api from './api'

// Cada função devolve só o conteúdo de "data" do envelope da API.
// Erros (status 4xx/5xx) chegam como exceção do Axios e são tratados nas páginas.

async function obterDados(promessa) {
  const resposta = await promessa
  return resposta.data.data
}

export const plataformaApi = {
  listar: () => obterDados(api.get('/Plataforma')),
  buscarPorId: (id) => obterDados(api.get(`/Plataforma/${id}`)),
  inserir: (plataforma) => obterDados(api.post('/Plataforma', plataforma)),
  alterar: (id, plataforma) => obterDados(api.put(`/Plataforma/${id}`, plataforma)),
  excluir: (id) => api.delete(`/Plataforma/${id}`),
}

export const generoApi = {
  listar: () => obterDados(api.get('/Genero')),
}

export const condicaoJogoApi = {
  listarCondicoes: () => obterDados(api.get('/CondicaoJogo')),
}

// Nome alternativo usado no roteiro para as tabelas de domínio.
export const tabelaTipoApi = condicaoJogoApi

export const jogoApi = {
  /** filtros: { busca, idPlataforma, idGenero, pagina, tamanhoPagina, incluirInativos } */
  listar: (filtros = {}) => obterDados(api.get('/Jogo', { params: filtros })),
  buscarPorId: (id) => obterDados(api.get(`/Jogo/${id}`)),
  inserir: (jogo) => obterDados(api.post('/Jogo', jogo)),
  alterar: (id, jogo) => obterDados(api.put(`/Jogo/${id}`, jogo)),
  /** Exclusão lógica: o jogo fica inativo (DELETE /Jogo/{id}). */
  desativar: (id) => api.delete(`/Jogo/${id}`),
}

export const autenticacaoApi = {
  entrar: (email, senha) => obterDados(api.post('/Autenticacao/login', { email, senha })),
  /** C01: cria a conta de cliente e já devolve a sessão iniciada (HTTP 201). */
  cadastrar: (nome, email, senha) => obterDados(api.post('/Autenticacao/cadastrar', { nome, email, senha })),
  sair: () => api.post('/Autenticacao/logout'),
  usuarioAtual: () => obterDados(api.get('/Autenticacao/eu')),
}
