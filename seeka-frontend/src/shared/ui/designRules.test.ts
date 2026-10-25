// @vitest-environment node
import { ESLint } from 'eslint'
import { describe, expect, it } from 'vitest'

const eslint = new ESLint()

async function messagesFor(code: string, filePath = 'src/features/jobs/Sample.tsx') {
  const [result] = await eslint.lintText(code, { filePath })
  return result.messages.map((message) => message.message)
}

describe('design system rules', () => {
  it('rejects Tailwind palette colors in class names', async () => {
    expect(await messagesFor(`export const a = 'rounded-xl bg-emerald-700 px-4'`)).toContainEqual(
      expect.stringMatching(/design token/i),
    )
    expect(await messagesFor('export const b = `border ${"x"} text-slate-500`')).toContainEqual(
      expect.stringMatching(/design token/i),
    )
    expect(await messagesFor(`export const c = 'bg-white text-black'`)).toContainEqual(
      expect.stringMatching(/design token/i),
    )
  })

  it('accepts token classes', async () => {
    expect(
      await messagesFor(
        `export const a = 'bg-primary text-primary-foreground border-border text-muted-foreground bg-success/10'`,
      ),
    ).toEqual([])
  })

  it('applies the color rule to shared UI too', async () => {
    expect(
      await messagesFor(`export const a = 'text-red-600'`, 'src/shared/ui/patterns/Sample.tsx'),
    ).toContainEqual(expect.stringMatching(/design token/i))
  })

  it('keeps features on the public shared/ui entry point', async () => {
    expect(
      await messagesFor(`import { Button } from '@/shared/ui/primitives/button'\nexport { Button }`),
    ).toContainEqual(expect.stringMatching(/@\/shared\/ui/))
    expect(
      await messagesFor(`import { Select } from 'radix-ui'\nexport { Select }`, 'src/app/Sample.tsx'),
    ).toContainEqual(expect.stringMatching(/@\/shared\/ui/))
    expect(await messagesFor(`import { Button } from '@/shared/ui'\nexport { Button }`)).toEqual([])
  })
})
