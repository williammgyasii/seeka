import { useState } from 'react'
import type { JobDetail } from '../lib/api'
import { Badge, Button, Input, Textarea } from './ui'

type Draft = JobDetail['drafts'][number]

export function DraftEditor({
  draft,
  onSave,
  onRecordCopy,
}: {
  draft: Draft
  onSave: (value: { subject?: string; body: string; isApproved: boolean }) => void
  onRecordCopy: () => void
}) {
  const [subject, setSubject] = useState(draft.subject ?? '')
  const [body, setBody] = useState(draft.body)

  const save = (isApproved: boolean) =>
    onSave({
      subject: subject || undefined,
      body,
      isApproved,
    })

  return (
    <div className="rounded-xl border border-slate-200 p-4">
      <div className="mb-3 flex items-center justify-between">
        <Badge tone="blue">{label(draft.channel)}</Badge>
        <Badge tone={draft.isApproved ? 'green' : 'amber'}>
          {draft.isApproved ? 'Approved' : 'Review required'}
        </Badge>
      </div>
      {draft.subject !== undefined && (
        <label className="mb-3 grid gap-1.5 text-sm font-medium">
          Subject
          <Input value={subject} onChange={(event) => setSubject(event.target.value)} />
        </label>
      )}
      <label className="grid gap-1.5 text-sm font-medium">
        Draft body
        <Textarea
          className="min-h-40"
          value={body}
          onChange={(event) => setBody(event.target.value)}
        />
      </label>
      <div className="mt-4 flex flex-wrap gap-2">
        <Button variant="secondary" onClick={() => save(false)}>
          Save edits
        </Button>
        <Button onClick={() => save(true)}>Approve draft</Button>
        <Button
          variant="ghost"
          onClick={async () => {
            await navigator.clipboard?.writeText(body)
            onRecordCopy()
          }}
        >
          Copy & record
        </Button>
      </div>
    </div>
  )
}

function label(value: string) {
  return value
    .replaceAll('_', ' ')
    .replace(/([a-z])([A-Z])/g, '$1 $2')
    .replace(/^./, (character) => character.toUpperCase())
}
