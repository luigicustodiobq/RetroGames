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
}
