import { useState, type DragEvent, type FormEvent } from 'react'
import { FileText, CloudUpload } from 'lucide-react'
import type { Resume } from '@/shared/api'
import { cn } from '@/shared/lib/utils'
import { Button, ErrorMessage, SectionCard } from '@/shared/ui'
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
  const [dragging, setDragging] = useState(false)
  const [upload, mutation] = useUploadResumeMutation()

  const choose = (file: File | undefined) =>
    setChoice(file ? { file, idempotencyKey: crypto.randomUUID() } : null)

  const drop = (event: DragEvent<HTMLLabelElement>) => {
    event.preventDefault()
    setDragging(false)
    const file = event.dataTransfer.files[0]
    if (file) choose(file)
  }

  const leave = (event: DragEvent<HTMLLabelElement>) => {
    // Moving over the icon or text fires dragleave on the zone; ignore those.
    if (!event.currentTarget.contains(event.relatedTarget as Node | null)) setDragging(false)
  }

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
        <label
          onDragOver={(event) => {
            event.preventDefault()
            setDragging(true)
          }}
          onDragLeave={leave}
          onDrop={drop}
          className={cn(
            'flex cursor-pointer flex-col items-center gap-3 rounded-lg border-2 border-dashed px-6 py-10 text-center transition-colors',
            'focus-within:ring-2 focus-within:ring-ring focus-within:ring-offset-2',
            dragging
              ? 'border-success bg-highlight/20'
              : 'border-success/40 bg-success/5 hover:border-success/70 hover:bg-success/10',
          )}
        >
          <input
            type="file"
            accept="application/pdf,.pdf"
            aria-label="Résumé (PDF)"
            className="sr-only"
            onChange={(event) => choose(event.target.files?.[0])}
          />
          <span className="flex size-12 items-center justify-center rounded-full bg-success/10 text-success">
            {choice ? <FileText className="size-6" /> : <CloudUpload className="size-6" />}
          </span>
          {choice ? (
            <>
              <span className="text-sm font-medium">{choice.file.name}</span>
              <span className="text-xs text-muted-foreground">Drop or click to choose another</span>
            </>
          ) : (
            <>
              <span className="text-sm font-medium">Drag your résumé here</span>
              <span className="text-xs text-muted-foreground">
                or <span className="font-medium text-success underline underline-offset-2">browse files</span>
              </span>
            </>
          )}
        </label>
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
