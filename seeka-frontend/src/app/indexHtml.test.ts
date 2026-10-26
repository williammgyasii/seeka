import { describe, expect, it } from 'vitest'
import html from '../../index.html?raw'

describe('index.html', () => {
  it('keeps the app out of search results', () => {
    expect(html).toMatch(/<meta\s+name="robots"\s+content="noindex"\s*\/?>/)
  })
})
