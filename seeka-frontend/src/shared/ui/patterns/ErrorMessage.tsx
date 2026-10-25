import { CircleAlert } from 'lucide-react'
import { errorMessage } from '@/shared/api'
import { Alert, AlertDescription } from '../primitives/alert'

export function ErrorMessage({ error }: { error: unknown }) {
  return (
    <Alert variant="destructive">
      <CircleAlert />
      <AlertDescription>{errorMessage(error)}</AlertDescription>
    </Alert>
  )
}
