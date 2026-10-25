import { Outlet } from 'react-router-dom'
import { useGetSessionQuery } from '@/shared/api'
import { Brand } from '@/shared/ui'
import { SignOutButton } from './SignOutButton'

/** Full-page shell for setup: no app navigation until onboarding is done. */
export function OnboardingLayout() {
  const session = useGetSessionQuery()
  return (
    <div className="min-h-screen bg-muted/40">
      <header className="flex h-16 items-center justify-between border-b bg-background px-5 lg:px-8">
        <Brand />
        <div className="flex items-center gap-3">
          <p className="hidden text-sm text-muted-foreground sm:block">{session.data?.email}</p>
          <SignOutButton withLabel />
        </div>
      </header>
      <main className="mx-auto max-w-4xl p-5 lg:p-10">
        <Outlet />
      </main>
    </div>
  )
}
