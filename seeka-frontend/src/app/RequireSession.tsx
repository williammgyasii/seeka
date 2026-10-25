import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useGetSessionQuery } from '@/shared/api'
import { ErrorMessage, Loading } from '@/shared/ui'

/**
 * Chooses the page for the visitor's session. This is user experience, not security:
 * the API's [Authorize] is the lock, so bypassing this only yields 401s.
 */
export function RequireSession() {
  const location = useLocation()
  const session = useGetSessionQuery()

  if (session.isLoading)
    return (
      <div className="mx-auto max-w-3xl p-10">
        <Loading />
      </div>
    )
  if (session.error && !isUnauthorized(session.error))
    return (
      <div className="mx-auto max-w-3xl p-10">
        <ErrorMessage error={session.error} />
      </div>
    )
  if (!session.data)
    return (
      <Navigate to="/login" replace state={{ from: location.pathname + location.search }} />
    )
  const onOnboarding = location.pathname === '/onboarding'
  if (!session.data.onboardingComplete && !onOnboarding)
    return <Navigate to="/onboarding" replace />
  if (session.data.onboardingComplete && onOnboarding) return <Navigate to="/dashboard" replace />
  return <Outlet />
}

function isUnauthorized(error: unknown) {
  return typeof error === 'object' && error !== null && 'status' in error && error.status === 401
}
