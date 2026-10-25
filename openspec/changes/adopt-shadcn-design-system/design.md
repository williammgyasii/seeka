## Context

React 19, Vite, Tailwind v4 (CSS-first config, no `tailwind.config.js`), the
`@/` alias to `src/`, and the feature-folder rules in
`.cursor/rules/frontend-architecture.mdc`. The shared UI is two files: about 80
palette classes in `primitives.tsx` and `layout.tsx`, plus the rest spread
across pages.

## Decisions

### Three layers, one import path

```
src/index.css             tokens: --background, --primary, --muted, --destructive,
                          --success, --warning, --info, --border, --ring, --radius ...
     |  @theme inline maps them to bg-primary, text-muted-foreground, ...
     v
shared/ui/primitives/     shadcn components, built only from tokens
     v
shared/ui/patterns/       Seeka compositions (PageHeader, StatusBadge, Field ...)
     v
shared/ui/index.ts  --->  features/*, app/*
```

- **Tokens** change when the brand changes. The brand palette change edits only
  `index.css`.
- **Primitives** change when shadcn changes. We may edit them (shadcn's model is
  that you own the code), but Seeka-specific meaning goes in patterns, so
  re-adding a primitive with the CLI doesn't lose it.
- **Patterns** change when Seeka's UI language changes.

### Semantic tones instead of colors

`StatusBadge tone="success" | "warning" | "danger" | "info" | "neutral"`
replaces `Badge tone="green" | "amber" | ...`. A caller says what a status
*means*, and the tokens decide how it looks.

| Old tone | New tone | Used for |
|---|---|---|
| green | success | eligible, approved, simulated plan |
| amber | warning | needs confirmation, draft pending |
| red | danger | not eligible |
| blue | info | fit score, draft channel |
| slate | neutral | unknown, counts |

### Button variants

Use shadcn's names: `primary` becomes `default`, `secondary` (white with a
border) becomes `outline`, and `ghost` stays `ghost`.

### Enforcement with ESLint, not review

- `no-restricted-syntax` matches string literals and template parts that
  contain a Tailwind palette color class (`<utility>-<palette>-<shade>`, or
  `bg-white`, `text-black`, and so on) and reports "Use a design token class."
- `no-restricted-imports` in `src/features/**` and `src/app/**` rejects
  `@/shared/ui/*` deep imports and `radix-ui` or `@radix-ui/*`.
- A Vitest test lints small code samples with the real config, so removing a
  rule fails a test.

### Stage picker

The native `<select>` becomes shadcn `Select` (Radix). The label stays
associated through `aria-labelledby`, so it stays accessible.

## Risks

- **Full restyle.** Every page changes visually. The behavior safety net is
  the existing role- and label-based tests, plus a browser pass over every
  screen.
- **CLI drift.** The shadcn CLI changes often (the `--base` flag is new). We
  pin what it generates by committing the files and avoid `--overwrite`.
