import { useState } from 'react'
import type { Draft } from '@/shared/api'
import { humanize } from '@/shared/lib/format'
import { Button, Field, Input, StatusBadge, Textarea } from '@/shared/ui'
import type { DraftChanges } from './outreachApi'

export function DraftEditor({
  draft,
  onSave,
  onRecordCopy,
}: {
  draft: Draft
  onSave: (value: DraftChanges) => void
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
    <div className="grid gap-3 rounded-lg border p-4">
      <div className="flex items-center justify-between">
        <StatusBadge tone="info">{humanize(draft.channel)}</StatusBadge>
        <StatusBadge tone={draft.isApproved ? 'success' : 'warning'}>
          {draft.isApproved ? 'Approved' : 'Review required'}
        </StatusBadge>
      </div>
      {draft.subject !== undefined && (
        <Field label="Subject">
          <Input value={subject} onChange={(event) => setSubject(event.target.value)} />
        </Field>
      )}
      <Field label="Draft body">
        <Textarea
          className="min-h-40"
          value={body}
          onChange={(event) => setBody(event.target.value)}
        />
      </Field>
      <div className="mt-1 flex flex-wrap gap-2">
        <Button variant="outline" onClick={() => save(false)}>
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