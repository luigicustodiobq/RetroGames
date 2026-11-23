// Tradução dos erros do Axios para mensagens exibidas na tela.
// A API devolve sempre o envelope { responseCode, errorMessage, data }.

/** Mensagem geral do erro, pronta para exibir ao usuário. */
export function obterMensagemErro(erro) {
  if (!erro?.response) {
    return 'Não foi possível conectar ao servidor.'
  }
  return erro.response.data?.errorMessage || 'Ocorreu um erro inesperado.'
}
