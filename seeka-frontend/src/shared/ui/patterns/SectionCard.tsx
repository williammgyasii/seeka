import type { ReactNode } from 'react'
import {
  Card,
  CardAction,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '../primitives/card'

export function SectionCard({
  title,
  description,
  action,
  className,
  children,
}: {
  title: string
  description: string
  action?: ReactNode
  className?: string
  children?: ReactNode
}) {
  return (
    <Card className={className}>
      <CardHeader>
        <CardTitle className="text-lg font-semibold tracking-tight">{title}</CardTitle>
        <CardDescription>{description}</CardDescription>
        {action && <CardAction>{action}</CardAction>}
      </CardHeader>
      {children && <CardContent className="grid gap-4">{children}</CardContent>}
    </Card>
  )
}
