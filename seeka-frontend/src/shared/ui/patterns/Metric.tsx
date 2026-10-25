import { Card, CardContent } from '../primitives/card'

export function Metric({ label, value, detail }: { label: string; value: number; detail: string }) {
  return (
    <Card>
      <CardContent>
        <p className="text-sm font-medium text-muted-foreground">{label}</p>
        <p className="mt-3 text-3xl font-semibold tracking-tight">{value}</p>
        <p className="mt-2 text-xs text-muted-foreground">{detail}</p>
      </CardContent>
    </Card>
  )
}
