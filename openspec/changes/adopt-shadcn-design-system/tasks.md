## 1. Failing tests

- [x] 1.1 Add `designRules.test.ts` that lints samples with the real ESLint config: palette classes fail, token classes pass, deep `@/shared/ui/...` and Radix imports from a feature fail, and verify it fails

## 2. Foundation

- [x] 2.1 Add `components.json` (Radix base, CSS variables, aliases to `shared/ui/primitives` and `shared/lib/utils`), run shadcn init, and add the `success`, `warning`, and `info` tokens
- [x] 2.2 Add the primitives: button, input, textarea, label, card, badge, select, alert, skeleton
- [x] 2.3 Add the ESLint rules and make the design-rules test pass

## 3. Patterns and pages

- [x] 3.1 Move the Seeka components into `shared/ui/patterns/` on top of the primitives and tokens; `index.ts` exports primitives and patterns
- [x] 3.2 Restyle `app/` and every feature page with token classes, `StatusBadge`, and shadcn variants; the stage picker uses `Select`

## 4. Delivery

- [x] 4.1 Update `.cursor/rules/frontend-architecture.mdc` and the README
- [x] 4.2 Run the tests, lint, and build, then check every screen in the running app
