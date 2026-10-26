import { useState, type ReactNode } from 'react'
import { Plus, Trash2 } from 'lucide-react'
import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
  Badge,
  Button,
} from '@/shared/ui'

export type ItemSummary = { title: string; detail?: string }

type Items = {
  itemName: string
  keys: string[]
  onRemove: (index: number) => void
  renderItem: (index: number) => ReactNode
}

/**
 * A repeated section, keyed by the field-array id so a removal keeps inputs
 * aligned. Given `summarize`, items collapse into an accordion; short rows
 * (languages, links) leave it out and stay open.
 */
export function ItemList({
  title,
  addLabel,
  onAdd,
  emptyText,
  error,
  summarize,
  hasError,
  ...items
}: Items & {
  title: string
  addLabel: string
  onAdd: () => void
  emptyText: string
  error?: string
  summarize?: (index: number) => ItemSummary
  hasError?: (index: number) => boolean
}) {
  return (
    <section className="grid gap-3">
      <h3 className="text-sm font-semibold">{title}</h3>
      {items.keys.length === 0 && <p className="text-sm text-muted-foreground">{emptyText}</p>}
      {summarize ? (
        <CollapsedItems {...items} summarize={summarize} hasError={hasError ?? (() => false)} />
      ) : (
        <OpenItems {...items} />
      )}
      {error && <p className="text-xs text-destructive">{error}</p>}
      <div>
        <Button type="button" variant="outline" size="sm" onClick={onAdd}>
          <Plus aria-hidden />
          {addLabel}
        </Button>
      </div>
    </section>
  )
}

function OpenItems({ itemName, keys, onRemove, renderItem }: Items) {
  return keys.map((key, index) => (
    <fieldset key={key} className="grid gap-4 rounded-lg border border-border p-4">
      <legend className="px-1 text-sm font-medium">
        {itemName} {index + 1}
      </legend>
      {renderItem(index)}
      <RemoveButton onClick={() => onRemove(index)} />
    </fieldset>
  ))
}

/**
 * Items start closed. One the user adds opens at once, and one with an error
 * stays open until it is fixed, so a rejected save never hides its reason.
 */
function CollapsedItems({
  itemName,
  keys,
  onRemove,
  renderItem,
  summarize,
  hasError,
}: Items & { summarize: (index: number) => ItemSummary; hasError: (index: number) => boolean }) {
  const [open, setOpen] = useState<string[]>([])
  const [seen, setSeen] = useState(keys)
  const added = keys.filter((key) => !seen.includes(key))
  if (added.length > 0) {
    setSeen(keys)
    setOpen([...open, ...added])
  }
  const failing = keys.filter((_, index) => hasError(index))
  const value = [...new Set([...open, ...failing])]

  return (
    <Accordion type="multiple" value={value} onValueChange={setOpen} className="grid gap-2">
      {keys.map((key, index) => {
        const summary = summarize(index)
        return (
          <AccordionItem key={key} value={key}>
            <AccordionTrigger>
              <span className="grid min-w-0 gap-0.5">
                <span className="truncate">{summary.title}</span>
                {summary.detail && (
                  <span className="text-xs font-normal text-muted-foreground">
                    {summary.detail}
                  </span>
                )}
              </span>
              {failing.includes(key) && (
                <Badge variant="destructive" className="ml-auto">
                  Needs a fix
                </Badge>
              )}
            </AccordionTrigger>
            <AccordionContent>
              <fieldset className="grid gap-4">
                <legend className="sr-only">
                  {itemName} {index + 1}
                </legend>
                {renderItem(index)}
                <RemoveButton onClick={() => onRemove(index)} />
              </fieldset>
            </AccordionContent>
          </AccordionItem>
        )
      })}
    </Accordion>
  )
}

function RemoveButton({ onClick }: { onClick: () => void }) {
  return (
    <div className="flex justify-end">
      <Button type="button" variant="ghost" size="sm" onClick={onClick}>
        <Trash2 aria-hidden />
        Remove
      </Button>
    </div>
  )
}
