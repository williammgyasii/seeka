import type { ReactNode } from 'react'

export function Page({ children }: { children: ReactNode }) {
  return <div className="grid gap-7">{children}</div>
}
