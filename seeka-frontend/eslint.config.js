import js from '@eslint/js'
import globals from 'globals'
import reactHooks from 'eslint-plugin-react-hooks'
import reactRefresh from 'eslint-plugin-react-refresh'
import tseslint from 'typescript-eslint'
import { defineConfig, globalIgnores } from 'eslint/config'

const palette =
  'slate|gray|zinc|neutral|stone|red|orange|amber|yellow|lime|green|emerald|teal|cyan|sky|blue|indigo|violet|purple|fuchsia|pink|rose'
const colorUtilities =
  'bg|text|border|border-[xytrbl]|ring|ring-offset|outline|divide|from|via|to|fill|stroke|shadow|decoration|placeholder|accent|caret'
const paletteClass = new RegExp(
  `\\b(${colorUtilities})-((${palette})-\\d{2,3}|white|black)\\b|-\\[(#|rgba?\\(|oklch\\()`,
).source
const tokenMessage =
  'Use a design token class (bg-primary, text-muted-foreground, bg-success/10 ...) instead of a palette color. Tokens live in src/index.css.'
const uiEntryMessage = 'Import UI from @/shared/ui, the design system entry point.'

export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      js.configs.recommended,
      tseslint.configs.recommended,
      reactHooks.configs.flat.recommended,
      reactRefresh.configs.vite,
    ],
    languageOptions: {
      globals: globals.browser,
    },
  },
  {
    files: ['src/**/*.{ts,tsx}'],
    ignores: ['src/shared/ui/designRules.test.ts'],
    rules: {
      'no-restricted-syntax': [
        'error',
        { selector: `Literal[value=/${paletteClass}/]`, message: tokenMessage },
        { selector: `TemplateElement[value.raw=/${paletteClass}/]`, message: tokenMessage },
      ],
    },
  },
  {
    files: ['src/features/**/*.{ts,tsx}', 'src/app/**/*.{ts,tsx}'],
    rules: {
      'no-restricted-imports': [
        'error',
        {
          paths: [{ name: 'radix-ui', message: uiEntryMessage }],
          patterns: [
            { group: ['@/shared/ui/*'], message: uiEntryMessage },
            { group: ['@radix-ui/*'], message: uiEntryMessage },
          ],
        },
      ],
    },
  },
  {
    files: ['src/shared/ui/primitives/**/*.tsx'],
    rules: {
      'react-refresh/only-export-components': 'off',
    },
  },
])
