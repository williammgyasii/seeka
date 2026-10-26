import { useState, type FormEvent } from 'react'
import type { Resume } from '@/shared/api'
import { Button, ErrorMessage, Field, Input, SectionCard } from '@/shared/ui'
import { useUploadResumeMutation } from './resumeApi'

type Choice = { file: File; idempotencyKey: string }

export function ResumeUpload({
  onRead,
  onSkip,
}: {
  onRead: (resume: Resume) => void
  onSkip: () => void
}) {
  // One key per chosen file: a retry of the same file must reuse it so the
  // server replays the first result instead of storing and extracting again.
  const [choice, setChoice] = useState<Choice | null>(null)
  const [upload, mutation] = useUploadResumeMutation()

  const read = async (event: FormEvent) => {
    event.preventDefault()
    if (!choice) return
    const result = await upload(choice)
    if (result.data) onRead(result.data)
  }

  return (
    <SectionCard
      title="Start with your résumé"
      description="Seeka reads your résumé with AI and fills in your profile. You check every field before anything is saved."
    >
      <form className="grid gap-4" onSubmit={read}>
        <Field label="Résumé (PDF)">
          <Input
            type="file"
            accept="application/pdf,.pdf"
            onChange={(event) => {
              const file = event.target.files?.[0]
              setChoice(file ? { file, idempotencyKey: crypto.randomUUID() } : null)
            }}
          />
        </Field>
        <p className="text-xs text-muted-foreground">
          PDF up to 5 MB. The file is kept privately with your profile.
        </p>
        {mutation.error && <ErrorMessage error={mutation.error} />}
        <div className="flex flex-wrap gap-3">
          <Button size="lg" disabled={!choice || mutation.isLoading}>
            {mutation.isLoading ? 'Reading your résumé…' : 'Read my résumé'}
          </Button>
          <Button type="button" variant="ghost" size="lg" onClick={onSkip}>
            Skip, I'll type it
          </Button>
        </div>
      </form>
    </SectionCard>
  )
}
