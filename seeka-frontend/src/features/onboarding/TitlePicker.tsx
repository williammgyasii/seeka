import { XIcon } from 'lucide-react'
import { useEffect, useId, useState, type KeyboardEvent } from 'react'
import { Badge, Input, Label } from '@/shared/ui'
import { useSearchJobTitlesQuery } from './onboardingApi'

const minQueryLength = 2
const searchDelayMs = 250

/**
 * Picks job titles from the server's ESCO list: type to search, choose with a
 * click or the arrow keys and Enter. Only listed titles can be added.
 */
export function TitlePicker({
  label,
  value,
  onChange,
  error,
}: {
  label: string
  value: string[]
  onChange: (titles: string[]) => void
  error?: string
}) {
  const inputId = useId()
  const listId = useId()
  const [query, setQuery] = useState('')
  const [term, setTerm] = useState('')
  const [dismissed, setDismissed] = useState(false)
  const [active, setActive] = useState(0)

  useEffect(() => {
    const timer = setTimeout(() => setTerm(query.trim()), searchDelayMs)
    return () => clearTimeout(timer)
  }, [query])

  const { data = [] } = useSearchJobTitlesQuery(term, { skip: term.length < minQueryLength })
  const options = term.length < minQueryLength ? [] : data.filter((title) => !value.includes(title))
  const open = !dismissed && options.length > 0
  const activeIndex = Math.min(active, options.length - 1)

  function add(title: string) {
    onChange([...value, title])
    setQuery('')
    setTerm('')
    setActive(0)
  }

  function onKeyDown(event: KeyboardEvent<HTMLInputElement>) {
    if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
      event.preventDefault()
      const step = event.key === 'ArrowDown' ? 1 : -1
      setActive((activeIndex + step + options.length) % Math.max(options.length, 1))
    } else if (event.key === 'Enter') {
      event.preventDefault()
      if (open) add(options[activeIndex])
    } else if (event.key === 'Escape') {
      setDismissed(true)
    }
  }

  return (
    <div className="grid gap-2">
      <Label htmlFor={inputId}>{label}</Label>
      {value.length > 0 && (
        <ul className="flex flex-wrap gap-2" aria-label="Chosen titles">
          {value.map((title) => (
            <li key={title}>
              <Badge variant="secondary" className="gap-1 pr-1">
                {title}
                <button
                  type="button"
                  aria-label={`Remove ${title}`}
                  className="rounded-sm p-0.5 hover:bg-foreground/10"
                  onClick={() => onChange(value.filter((item) => item !== title))}
                >
                  <XIcon className="size-3" />
                </button>
              </Badge>
            </li>
          ))}
        </ul>
      )}
      <div className="relative">
        <Input
          id={inputId}
          role="combobox"
          aria-expanded={open}
          aria-controls={listId}
          aria-autocomplete="list"
          aria-activedescendant={open ? `${listId}-${activeIndex}` : undefined}
          aria-invalid={Boolean(error)}
          autoComplete="off"
          placeholder="Search, for example: HR manager"
          value={query}
          onChange={(event) => {
            setQuery(event.target.value)
            setDismissed(false)
          }}
          onKeyDown={onKeyDown}
          onBlur={() => setDismissed(true)}
        />
        <ul
          id={listId}
          role="listbox"
          aria-label={label}
          hidden={!open}
          className="absolute z-10 mt-1 max-h-60 w-full overflow-y-auto rounded-md border bg-popover p-1 shadow-md"
        >
          {options.map((title, index) => (
            <li
              key={title}
              id={`${listId}-${index}`}
              role="option"
              aria-selected={index === activeIndex}
              className="cursor-pointer rounded-sm px-2 py-1.5 text-sm aria-selected:bg-accent aria-selected:text-accent-foreground"
              onMouseDown={(event) => event.preventDefault()}
              onMouseEnter={() => setActive(index)}
              onClick={() => add(title)}
            >
              {title}
            </li>
          ))}
        </ul>
      </div>
      {error && <span className="text-xs text-destructive">{error}</span>}
      <p className="text-xs text-muted-foreground">
        Titles come from ESCO, the European Commission&apos;s list of occupations.
      </p>
    </div>
  )
}
