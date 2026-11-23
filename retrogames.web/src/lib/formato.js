// Formatação de valores para exibição (padrão brasileiro).

const formatadorPreco = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
})

const formatadorDataHora = new Intl.DateTimeFormat('pt-BR', {
  day: '2-digit',
  month: '2-digit',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
})

/** 149.9 -> "R$ 149,90". */
export function formatarPreco(valor) {
  return formatadorPreco.format(Number(valor) || 0)
}

/**
 * "2026-11-23T14:05:00Z" -> "23/11/2026 11:05" (no fuso do navegador).
 * Datas sem fuso vindas da API são tratadas como UTC.
 */
export function formatarDataHora(dataIso) {
  if (!dataIso) {
    return ''
  }
  const temFuso = /[zZ]|[+-]\d{2}:?\d{2}$/.test(dataIso)
  const data = new Date(temFuso ? dataIso : `${dataIso}Z`)
  if (Number.isNaN(data.getTime())) {
    return ''
  }
  return formatadorDataHora.format(data).replace(',', '')
}
