import { useState } from 'react'
import { useGetSessionQuery } from '@/shared/api'
import { Button, ErrorMessage, Page, PageHeader, SectionCard } from '@/shared/ui'
import { ChangeEmailForm } from './ChangeEmailForm'
import { useResendVerificationEmailMutation } from './verificationApi'

export function VerifyEmailPage() {
  const session = useGetSessionQuery()
  const [resend, resent] = useResendVerificationEmailMutation()
  const [correcting, setCorrecting] = useState(false)

  return (
    <Page>
      <PageHeader
        eyebrow="Verify your email"
        title="Check your inbox."
        description="Open the link in the email to confirm your address. Your workspace unlocks as soon as you do."
      />
      <SectionCard
        title="Verification email"
        description="The link expires in 24 hours. Check spam if it hasn't arrived in a minute."
      >
        <p>
          We sent a link to <strong>{session.data?.email}</strong>.
        </p>
        <div className="flex flex-wrap gap-3">
          <Button onClick={() => resend()} disabled={resent.isLoading}>
            Send a new link
          </Button>
          <Button variant="outline" onClick={() => setCorrecting(true)}>
            Wrong address?
          </Button>
        </div>
        {resent.isSuccess && (
          <p role="status" className="text-sm text-muted-foreground">
            We sent a new link.
          </p>
        )}
        {resent.error && <ErrorMessage error={resent.error} />}
        {correcting && <ChangeEmailForm onChanged={() => setCorrecting(false)} />}
      </SectionCard>
    </Page>
  )
}
