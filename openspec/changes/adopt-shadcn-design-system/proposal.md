## Why

The UI primitives in `shared/ui/primitives.tsx` borrow shadcn's tools (`cva`,
`Slot`, `tailwind-merge`) but aren't a design system. There are about 80
hard-coded palette classes (`bg-emerald-700`, `border-slate-200`) across 13
files, including the feature pages, and there are no design tokens. Changing
the brand means editing every page, and nothing stops a new page from
inventing its own colors. The shadcn CLI can't add components because there is
no `components.json`.

## What Changes

- Initialize shadcn (Radix primitives, CSS variables) with its default neutral
  theme. The app takes the neutral look now. A Seeka brand palette is a
  separate, later change that edits the tokens only.
- Design tokens live in `src/index.css` as CSS variables, mapped into Tailwind
  v4 with `@theme inline`. Seeka adds three semantic tokens to shadcn's set:
  `success`, `warning`, and `info` (used for eligibility and draft status).
- `shared/ui` is split by owner:
  - `shared/ui/primitives/`: shadcn-generated components (button, input,
    textarea, label, card, badge, select, alert, skeleton). shadcn's aliases
    in `components.json` point here.
  - `shared/ui/patterns/`: Seeka's own compositions built only from
    primitives and tokens (PageHeader, Metric, EmptyState, Field,
    StatusBadge, EligibilityBadge, ErrorMessage, Loading, Brand, Page,
    SectionTitle).
  - `shared/ui/index.ts` is the only import path for features and `app/`.
- Every page uses token classes (`bg-primary`, `text-muted-foreground`,
  `border-border`) instead of palette classes.
- The job stage picker becomes the shadcn `Select`.
- ESLint enforces the system:
  - It rejects Tailwind palette classes (`emerald-700`, `bg-white`, ...) in
    any `.ts` or `.tsx` file.
  - It rejects deep imports from `@/shared/ui/...` and direct Radix imports
    outside `shared/ui`.

## Not changing

- The API proxy: the browser already calls same-origin `/api` in dev (Vite),
  Docker (nginx), and production (Cloudflare Pages Function with a service
  binding).
- Routes, data flow, and form wiring (`react-hook-form` stays as is).
- No dark mode, no Storybook.

## Impact

- `seeka-frontend`:
  - Adds `components.json`, the shadcn dependencies (`radix-ui`,
    `tw-animate-css`), `src/index.css` tokens, and `shared/ui/primitives` and
    `shared/ui/patterns`.
  - Restyles every page and `eslint.config.js`.
- `.cursor/rules/frontend-architecture.mdc` gains the design-system rules.
