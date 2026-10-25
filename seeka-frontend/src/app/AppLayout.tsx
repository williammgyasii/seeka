import {
  BriefcaseBusiness,
  CircleUserRound,
  LayoutDashboard,
  LogOut,
  Search,
  Settings,
} from 'lucide-react'
import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useSignOutMutation } from '@/features/auth/authApi'
import { useGetSessionQuery } from '@/shared/api'
import { cn } from '@/shared/lib/utils'
import { Brand, Button } from '@/shared/ui'

const navItems = [
  { to: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/jobs/import', label: 'Job discovery', icon: Search },
  { to: '/pipeline', label: 'Pipeline', icon: BriefcaseBusiness },
  { to: '/settings', label: 'Settings & billing', icon: Settings },
]

export function AppLayout() {
  const navigate = useNavigate()
  const session = useGetSessionQuery()
  const [signOut] = useSignOutMutation()

  return (
    <div className="min-h-screen bg-muted/40">
      <aside className="fixed inset-y-0 left-0 hidden w-64 border-r bg-sidebar p-5 text-sidebar-foreground lg:flex lg:flex-col">
        <Brand className="px-2 py-2" />
        <nav className="mt-8 grid gap-1">
          {navItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) =>
                cn(
                  'flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium',
                  isActive
                    ? 'bg-sidebar-accent text-sidebar-accent-foreground'
                    : 'text-muted-foreground hover:bg-sidebar-accent hover:text-sidebar-accent-foreground',
                )
              }
            >
              <item.icon size={18} />
              {item.label}
            </NavLink>
          ))}
        </nav>
        <div className="mt-auto rounded-xl bg-primary p-4 text-primary-foreground">
          <p className="text-xs font-semibold tracking-wider uppercase">Manual mode</p>
          <p className="mt-2 text-sm opacity-80">
            ATS, email, enrichment, AI, and billing providers are not connected.
          </p>
        </div>
      </aside>
      <div className="lg:pl-64">
        <header className="sticky top-0 z-20 flex h-16 items-center justify-between border-b bg-background/90 px-5 backdrop-blur lg:px-8">
          <Brand className="lg:hidden" />
          <nav className="flex gap-1 overflow-x-auto lg:hidden">
            {navItems.slice(0, 3).map((item) => (
              <NavLink key={item.to} to={item.to} className="rounded-lg p-2 text-muted-foreground">
                <item.icon size={19} />
                <span className="sr-only">{item.label}</span>
              </NavLink>
            ))}
          </nav>
          <div className="ml-auto hidden items-center gap-3 lg:flex">
            <div className="text-right">
              <p className="text-sm font-semibold">
                {session.data?.displayName ?? 'Your workspace'}
              </p>
              <p className="text-xs text-muted-foreground">{session.data?.email}</p>
            </div>
            <CircleUserRound className="text-muted-foreground" />
            <Button
              variant="ghost"
              size="icon"
              aria-label="Sign out"
              onClick={async () => {
                if (!(await signOut()).error) navigate('/login')
              }}
            >
              <LogOut />
            </Button>
          </div>
        </header>
        <main className="mx-auto max-w-7xl p-5 lg:p-8">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
