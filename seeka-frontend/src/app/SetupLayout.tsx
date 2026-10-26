import { Outlet } from 'react-router-dom'
import { useGetSessionQuery } from '@/shared/api'
import { Brand } from '@/shared/ui'
import { SignOutButton } from './SignOutButton'

/** Full-page shell for setup: no app navigation until verification and onboarding are done. */
export function SetupLayout() {
  const session = useGetSessionQuery()
  return (
    // Exactly one viewport tall: pages scroll inside <main>, and onboarding fills it.
    // dvh, not vh, so a phone's address bar can't push pinned buttons off screen.
    <div className="flex h-dvh flex-col bg-muted/40">
      <header className="flex h-16 shrink-0 items-center justify-between border-b bg-background px-5 lg:px-8">
        <Brand />
        <div className="flex items-center gap-3">
          <p className="hidden text-sm text-muted-foreground sm:block">{session.data?.email}</p>
          <SignOutButton withLabel />
        </div>
      </header>
      <main className="relative min-h-0 flex-1 overflow-y-auto">
        <div className="mx-auto flex h-full max-w-4xl flex-col p-4 lg:px-10 lg:py-6">
          <Outlet />
        </div>
      </main>
    </div>
  )
}
