// Tradução dos erros do Axios para mensagens exibidas na tela.
// A API devolve sempre o envelope { responseCode, errorMessage, data }.

/** Mensagem geral do erro, pronta para exibir ao usuário. */
export function obterMensagemErro(erro) {
  if (!erro?.response) {
    return 'Não foi possível conectar ao servidor.'
  }
  return erro.response.data?.errorMessage || 'Ocorreu um erro inesperado.'
}

/** "Nome" ou "IdsGeneros[0]" -> "nome" / "idsGeneros". */
function normalizarCampo(chave) {
  const semIndice = chave.replace(/\[\d+\]$/, '').replace(/^\$\./, '')
  return semIndice.charAt(0).toLowerCase() + semIndice.slice(1)
}

/**
 * Erros de validação por campo (responseCode 3): { campo: ["msg"] } -> { campo: "msg" }.
 * Para qualquer outro erro devolve {} (a página mostra só a mensagem geral).
 */
export function obterErrosDosCampos(erro) {
  const dados = erro?.response?.data
  if (dados?.responseCode !== 3 || !dados.data || typeof dados.data !== 'object') {
    return {}
  }

  const erros = {}
  for (const [chave, mensagens] of Object.entries(dados.data)) {
    const campo = normalizarCampo(chave)
    if (!erros[campo]) {
      erros[campo] = Array.isArray(mensagens) ? mensagens[0] : String(mensagens)
    }
  }
  return erros
}
