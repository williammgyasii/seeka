import { Alert, AlertDescription, Button, Page, PageHeader, SectionCard, StatusBadge } from '@/shared/ui'

const providers = ['AI extraction and writing', 'Contact enrichment', 'Email sending', 'Live billing']

export function SettingsPage() {
  return (
    <Page>
      <PageHeader
        eyebrow="Settings & billing"
        title="Your account and providers."
        description="Credential-backed features stay visibly off until a real provider is configured."
      />
      <div className="grid gap-6 lg:grid-cols-2">
        <SectionCard title="Connected providers" description="Server-side credentials only">
          {providers.map((provider) => (
            <div key={provider} className="flex items-center justify-between rounded-lg border p-4">
              <span className="font-medium">{provider}</span>
              <StatusBadge>Not connected</StatusBadge>
            </div>
          ))}
        </SectionCard>
        <SectionCard title="Subscription" description="Development environment">
          <Alert className="border-success/30 bg-success/10">
            <AlertDescription className="grid gap-3 text-success">
              <StatusBadge tone="success">Simulated development plan</StatusBadge>
              No live payment provider is implemented. Merchant eligibility for a Ghana-based
              business must be established before selecting one.
            </AlertDescription>
          </Alert>
          <Button variant="outline" size="lg" disabled>
            Export account data — coming next
          </Button>
          <Button variant="outline" size="lg" disabled>
            Delete account — coming next
          </Button>
        </SectionCard>
      </div>
    </Page>
  )
}
