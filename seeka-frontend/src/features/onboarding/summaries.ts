/** Fixed English month names, so headers don't change with the browser's locale. */
const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']

function month(value: string | undefined): string {
  const match = /^(\d{4})-(0[1-9]|1[0-2])$/.exec(value ?? '')
  return match ? `${months[Number(match[2]) - 1]} ${match[1]}` : ''
}

/** "Jun 2021 – Present" for the header of a job or volunteering role. */
export function monthRange(start: string | undefined, end: string | undefined, current: boolean) {
  const from = month(start)
  const to = current ? 'Present' : month(end)
  return from && to ? `${from} – ${to}` : from || (current ? '' : to)
}

export function yearRange(start: string | undefined, end: string | undefined) {
  return [start, end].map((year) => year?.trim()).filter(Boolean).join(' – ')
}

/** "HR Lead at Acme", or the placeholder until the user types either part. */
export function itemSummary(role: string | undefined, place: string | undefined, placeholder: string) {
  const what = role?.trim()
  const where = place?.trim()
  if (what && where) return `${what} at ${where}`
  return what || where || placeholder
}
