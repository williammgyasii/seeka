import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { errorMessage } from '@/shared/api'
import { Button, Field, Input } from '@/shared/ui'
import { useChangeEmailMutation } from './verificationApi'

const schema = z.object({ email: z.email('Enter a valid email address.') })

export function ChangeEmailForm({ onChanged }: { onChanged: () => void }) {
  const form = useForm<z.infer<typeof schema>>({ resolver: zodResolver(schema) })
  const [changeEmail, mutation] = useChangeEmailMutation()

  const submit = async ({ email }: z.infer<typeof schema>) => {
    const result = await changeEmail({ email: email.trim() })
    if (!result.error) onChanged()
  }

  return (
    <form className="grid gap-3 sm:grid-cols-[1fr_auto] sm:items-end" onSubmit={form.handleSubmit(submit)}>
      <Field
        label="New email address"
        error={
          form.formState.errors.email?.message ??
          (mutation.error ? errorMessage(mutation.error) : undefined)
        }
      >
        <Input type="email" autoComplete="email" {...form.register('email')} />
      </Field>
      <Button type="submit" disabled={mutation.isLoading}>
        Update address
      </Button>
    </form>
  )
}
