import { FieldData } from '@rc-component/form/lib/interface'

const normalizeName = (raw: string) => {
  const trimmed = (raw ?? '').trim()
  if (!trimmed) return trimmed
  return trimmed.charAt(0).toLowerCase() + trimmed.slice(1)
}

const normalizeDottedName = (raw: string) => {
  const trimmed = (raw ?? '').trim()
  if (!trimmed) return trimmed
  return trimmed
    .split('.')
    .filter(Boolean)
    .map((segment) =>
      segment ? segment.charAt(0).toLowerCase() + segment.slice(1) : segment,
    )
    .join('.')
}

const stripJsonPathPrefix = (key: string) => key.replace(/^\$\.?/, '')

const lastPathSegment = (key: string) => {
  const normalized = stripJsonPathPrefix(key)
  const segment = normalized.split('.').filter(Boolean).pop() ?? key
  return segment.replace(/\[.*\]$/, '')
}

const toFormErrors = (problemDetails: {
  [property: string]: string[] | string
}): FieldData[] => {
  if (!problemDetails) return []

  const orderedNames: string[] = []
  const nameToErrors = new Map<string, string[]>()

  for (const key of Object.keys(problemDetails)) {
    const rawErrors = problemDetails[key]
    const errors = Array.isArray(rawErrors) ? rawErrors : [rawErrors]

    const normalizedKey = stripJsonPathPrefix(key)
    const candidates = [
      key,
      normalizedKey,
      lastPathSegment(key),
      normalizeName(key),
      normalizeName(normalizedKey),
      normalizeName(lastPathSegment(key)),
      normalizeDottedName(key),
      normalizeDottedName(normalizedKey),
    ].filter(Boolean)

    for (const candidate of candidates) {
      const existing = nameToErrors.get(candidate)
      if (!existing) {
        orderedNames.push(candidate)
        nameToErrors.set(candidate, [...errors])
        continue
      }

      for (const message of errors) {
        if (!existing.includes(message)) {
          existing.push(message)
        }
      }
    }
  }

  return orderedNames.map((name) => ({ name, errors: nameToErrors.get(name)! }))
}

export default toFormErrors
