import { useEffect, useRef } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { useGetSessionQuery } from '@/shared/api'
import { Brand, Button, Card, CardContent, Loading } from '@/shared/ui'
import { useVerifyEmailMutation } from './verificationApi'

/** Public: the link may be opened on any device, signed in or not. */
export function ConfirmEmailPage() {
  const [params] = useSearchParams()
  const userId = params.get('userId')
  const token = params.get('token')
  const session = useGetSessionQuery()
  const [verify, result] = useVerifyEmailMutation()
  const requested = useRef(false)

  useEffect(() => {
    if (!userId || !token || requested.current) return
    requested.current = true
    verify({ userId, token })
  }, [userId, token, verify])

  const invalid = !userId || !token || result.isError

  return (
    <main className="grid min-h-screen place-items-center bg-muted/40 p-5">
      <Card className="w-full max-w-md px-3 py-7 sm:px-5">
        <CardContent className="grid gap-5">
          <Brand />
          {invalid ? (
            <>
              <h1 className="font-heading text-2xl font-semibold tracking-tight">
                This link has expired or is invalid.
              </h1>
              <p className="text-muted-foreground">
                Links last 24 hours, and only the newest one works. Request a new one.
              </p>
              <Button asChild>
                <Link to="/verify-email">Get a new link</Link>
              </Button>
            </>
          ) : result.isSuccess ? (
            <>
              <h1 className="font-heading text-2xl font-semibold tracking-tight">
                Your email is verified.
              </h1>
              {session.data ? (
                <Button asChild>
                  <Link to="/dashboard">Continue</Link>
                </Button>
              ) : session.isLoading ? null : (
                <Button asChild>
                  <Link to="/login">Sign in</Link>
                </Button>
              )}
            </>
          ) : (
            <Loading />
          )}
        </CardContent>
      </Card>
    </main>
  )
}
