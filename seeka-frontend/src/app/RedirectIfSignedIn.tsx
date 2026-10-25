import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useGetSessionQuery } from '@/shared/api'

/** Sends a signed-in visitor on sign-in to where they were going, or the dashboard. */
export function RedirectIfSignedIn() {
  const location = useLocation()
  const session = useGetSessionQuery()
  if (!session.data) return <Outlet />
  const from = (location.state as { from?: string } | null)?.from
  return <Navigate to={from ?? '/dashboard'} replace />
}
