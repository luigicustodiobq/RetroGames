import { Link } from 'react-router-dom'

/** Estado "carregando": avisa leitores de tela com role="status". */
export function Carregando({ texto = 'Carregando...' }) {
  return (
    <div className="rg-estado" role="status" aria-live="polite">
      <div className="spinner-border mb-3" aria-hidden="true" />
      <p className="text-body-secondary mb-0">{texto}</p>
    </div>
  )
}

/** Estado de erro: role="alert" e botão para tentar de novo. */
export function MensagemErro({ mensagem, aoTentarNovamente }) {
  return (
    <div className="rg-painel rg-estado" role="alert">
      <div className="rg-estado-icone" aria-hidden="true">⚠️</div>
      <h2 className="h5 mb-2">Algo deu errado</h2>
      <p className="text-body-secondary mb-4">{mensagem}</p>
      {aoTentarNovamente && (
        <button type="button" className="btn btn-primary" onClick={aoTentarNovamente}>
          Tentar novamente
        </button>
      )}
    </div>
  )
}

/**
 * Estado vazio: título, descrição e uma ação de retorno
 * (link com "para" ou botão com "aoClicar").
 */
export function EstadoVazio({ titulo, descricao, textoAcao, para, aoClicar, icone = '🕹️' }) {
  return (
    <div className="rg-painel rg-estado">
      <div className="rg-estado-icone" aria-hidden="true">{icone}</div>
      <h2 className="h5 mb-2">{titulo}</h2>
      {descricao && <p className="text-body-secondary mb-4">{descricao}</p>}
      {textoAcao && para && (
        <Link to={para} className="btn btn-outline-light">{textoAcao}</Link>
      )}
      {textoAcao && !para && aoClicar && (
        <button type="button" className="btn btn-outline-light" onClick={aoClicar}>{textoAcao}</button>
      )}
    </div>
  )
}
