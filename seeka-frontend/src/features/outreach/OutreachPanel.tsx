import { Mail, Users } from 'lucide-react'
import { useForm } from 'react-hook-form'
import type { JobDetail } from '@/shared/api'
import { Button, Field, Input, SectionCard, Textarea } from '@/shared/ui'
import {
  useAddContactMutation,
  useGenerateDraftsMutation,
  useRecordOutreachMutation,
  useUpdateDraftMutation,
  type ContactValues,
} from './outreachApi'
import { DraftEditor } from './DraftEditor'

export function OutreachPanel({ detail }: { detail: JobDetail }) {
  const savedJobId = detail.savedJob.id
  const contactForm = useForm<ContactValues>({
    defaultValues: {
      name: '',
      title: '',
      professionalProfileUrl: '',
      professionalEmail: '',
      relevanceReason: '',
    },
  })
  const [addContact] = useAddContactMutation()
  const [generateDrafts] = useGenerateDraftsMutation()
  const [updateDraft] = useUpdateDraftMutation()
  const [recordOutreach] = useRecordOutreachMutation()
  const saveContact = async (contact: ContactValues) => {
    if (!(await addContact({ savedJobId, contact })).error) contactForm.reset()
  }

  return (
    <SectionCard
      title="Contact research & outreach"
      description="Manual entry is active. Email finding and sending are not connected."
    >
      <form
        className="grid gap-3 rounded-lg bg-muted p-4 md:grid-cols-2"
        onSubmit={contactForm.handleSubmit(saveContact)}
      >
        <Field label="Contact name">
          <Input {...contactForm.register('name')} required />
        </Field>
        <Field label="Role / title">
          <Input {...contactForm.register('title')} required />
        </Field>
        <Field label="Professional profile URL">
          <Input type="url" {...contactForm.register('professionalProfileUrl')} />
        </Field>
        <Field label="Professional email">
          <Input type="email" {...contactForm.register('professionalEmail')} />
        </Field>
        <Field label="Why this person is relevant">
          <Textarea {...contactForm.register('relevanceReason')} required />
        </Field>
        <div className="flex items-end">
          <Button size="lg" className="w-full">
            <Users /> Add manual contact
          </Button>
        </div>
      </form>
      {detail.contacts.map((contact) => (
        <div
          key={contact.id}
          className="flex flex-wrap items-center justify-between gap-3 rounded-lg border p-4"
        >
          <div>
            <p className="font-semibold">{contact.name}</p>
            <p className="text-muted-foreground">
              {contact.title} · Email {contact.verificationStatus.toLowerCase()}
            </p>
            <p className="mt-1 text-xs text-muted-foreground">{contact.relevanceReason}</p>
          </div>
          <Button
            variant="outline"
            onClick={() => generateDrafts({ savedJobId, contactId: contact.id })}
          >
            <Mail /> Generate drafts
          </Button>
        </div>
      ))}
      {!!detail.drafts.length && (
        <div className="mt-3 grid gap-4">
          <div>
            <h3 className="font-semibold">Drafts ready for review</h3>
            <p className="text-muted-foreground">No message is sent automatically</p>
          </div>
          {detail.drafts.map((draft) => (
            <DraftEditor
              key={draft.id}
              draft={draft}
              onSave={(changes) => updateDraft({ savedJobId, draftId: draft.id, changes })}
              onRecordCopy={() => recordOutreach(savedJobId)}
            />
          ))}
        </div>
      )}
    </SectionCard>
  )
}
