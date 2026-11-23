/** Campo de quantidade com botões − e + (área de toque de 44px). */
export default function SeletorQuantidade({ id, valor, minimo = 1, maximo, aoMudar, rotulo = 'Quantidade', desabilitado = false }) {
  return (
    <div className="input-group rg-quantidade">
      <button
        type="button"
        className="btn btn-outline-light"
        onClick={() => aoMudar(valor - 1)}
        disabled={desabilitado || valor <= minimo}
        aria-label={`Diminuir ${rotulo.toLowerCase()}`}
      >
        −
      </button>
      <input
        id={id}
        type="number"
        inputMode="numeric"
        className="form-control"
        min={minimo}
        max={maximo}
        value={valor}
        disabled={desabilitado}
        aria-label={rotulo}
        onChange={(e) => aoMudar(Number(e.target.value))}
      />
      <button
        type="button"
        className="btn btn-outline-light"
        onClick={() => aoMudar(valor + 1)}
        disabled={desabilitado || valor >= maximo}
        aria-label={`Aumentar ${rotulo.toLowerCase()}`}
      >
        +
      </button>
    </div>
  )
}
