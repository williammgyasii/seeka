import { LogOut } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import { useSignOutMutation } from '@/features/auth/authApi'
import { Button } from '@/shared/ui'

export function SignOutButton({ withLabel = false }: { withLabel?: boolean }) {
  const navigate = useNavigate()
  const [signOut] = useSignOutMutation()
  return (
    <Button
      variant="ghost"
      size={withLabel ? 'default' : 'icon'}
      aria-label="Sign out"
      onClick={async () => {
        if (!(await signOut()).error) navigate('/login')
      }}
    >
      <LogOut />
      {withLabel && 'Sign out'}
    </Button>
  )
}
