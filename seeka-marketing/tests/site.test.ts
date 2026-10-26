import { existsSync, readFileSync } from 'node:fs'
import { join } from 'node:path'
import { parse, type HTMLElement } from 'node-html-parser'
import { describe, expect, it } from 'vitest'

const ORIGIN = 'https://seekajobs.tech'
const REGISTER = 'https://app.seekajobs.tech/login?mode=register'
const SIGN_IN = 'https://app.seekajobs.tech/login'
const dist = join(import.meta.dirname, '..', 'dist')

const guides = [
  '/guides/how-to-get-a-referral/',
  '/guides/find-the-hiring-manager/',
  '/guides/message-a-recruiter/',
  '/guides/no-response-to-applications/',
]
const pages = ['/', '/guides/', ...guides]

function read(file: string) {
  const path = join(dist, file)
  if (!existsSync(path)) throw new Error(`${file} was not built`)
  return readFileSync(path, 'utf8')
}

function page(path: string): HTMLElement {
  return parse(read(join(path, 'index.html')))
}

function meta(doc: HTMLElement, key: string) {
  return (
    doc.querySelector(`meta[name="${key}"]`) ?? doc.querySelector(`meta[property="${key}"]`)
  )?.getAttribute('content')
}

function jsonLd(doc: HTMLElement): Array<Record<string, unknown>> {
  return doc
    .querySelectorAll('script[type="application/ld+json"]')
    .flatMap((script) => [JSON.parse(script.text)].flat())
}

function links(doc: HTMLElement) {
  return doc.querySelectorAll('a').map((a) => a.getAttribute('href'))
}

describe.each(pages)('%s is search-ready', (path) => {
  const doc = page(path)

  it('has one h1, a title, and a description', () => {
    expect(doc.querySelectorAll('h1')).toHaveLength(1)
    expect(doc.querySelector('title')?.text.trim()).toBeTruthy()
    expect(meta(doc, 'description')?.length).toBeGreaterThan(50)
    expect(doc.querySelector('html')?.getAttribute('lang')).toBe('en')
  })

  it('has an absolute canonical URL on the apex', () => {
    expect(doc.querySelector('link[rel="canonical"]')?.getAttribute('href')).toBe(
      `${ORIGIN}${path}`,
    )
  })

  it('has Open Graph and Twitter card tags', () => {
    expect(meta(doc, 'og:title')).toBeTruthy()
    expect(meta(doc, 'og:description')).toBeTruthy()
    expect(meta(doc, 'og:url')).toBe(`${ORIGIN}${path}`)
    expect(meta(doc, 'og:image')).toMatch(/^https:\/\/seekajobs\.tech\/.+\.png$/)
    expect(meta(doc, 'twitter:card')).toBe('summary_large_image')
  })

  it('has parseable structured data', () => {
    expect(jsonLd(doc).length).toBeGreaterThan(0)
  })
})

describe('the site as a whole', () => {
  it('gives every page a unique title and description', () => {
    const docs = pages.map(page)
    const titles = docs.map((doc) => doc.querySelector('title')?.text)
    const descriptions = docs.map((doc) => meta(doc, 'description'))
    expect(new Set(titles).size).toBe(pages.length)
    expect(new Set(descriptions).size).toBe(pages.length)
  })

  it('ships the share image at 1200x630', () => {
    const png = readFileSync(join(dist, 'og.png'))
    expect(png.subarray(1, 4).toString()).toBe('PNG')
    expect([png.readUInt32BE(16), png.readUInt32BE(20)]).toEqual([1200, 630])
  })

  it('lets crawlers in and points them at the sitemap', () => {
    const robots = read('robots.txt')
    expect(robots).toMatch(/User-agent: \*\s+Allow: \//)
    expect(robots).toContain(`Sitemap: ${ORIGIN}/sitemap-index.xml`)
  })

  it('lists every page in the sitemap and leaves out the 404', () => {
    const sitemap = read('sitemap-0.xml')
    for (const path of pages) expect(sitemap).toContain(`<loc>${ORIGIN}${path}</loc>`)
    expect(sitemap).not.toContain('404')
  })

  it('answers unknown paths with a page that links home and to the guides', () => {
    const doc = parse(read('404.html'))
    expect(meta(doc, 'robots')).toBe('noindex')
    expect(links(doc)).toEqual(expect.arrayContaining(['/', '/guides/']))
  })
})

describe('the landing page', () => {
  const doc = page('/')

  it('states the promise above the fold', () => {
    const hero = doc.querySelector('header + main section, main section')
    expect(hero?.text).toMatch(/real person inside the company/i)
  })

  it('sends visitors to sign up and returning users to sign in', () => {
    expect(links(doc)).toContain(REGISTER)
    expect(links(doc)).toContain(SIGN_IN)
  })

  it('describes Seeka as an application', () => {
    const types = jsonLd(doc).map((item) => item['@type'])
    expect(types).toEqual(expect.arrayContaining(['Organization', 'WebSite', 'SoftwareApplication']))
  })

  it('says Seeka does not guarantee a job', () => {
    expect(doc.text).toContain('No job guarantee')
  })

  it('answers "Will Seeka get me a job?" first, on the page and for search', () => {
    const faq = jsonLd(doc).find((item) => item['@type'] === 'FAQPage')
    const [first] = faq?.mainEntity as Array<{ name: string; acceptedAnswer: { text: string } }>
    expect(first.name).toBe('Will Seeka get me a job?')
    expect(first.acceptedAnswer.text).toMatch(/^No\b/)
    expect(first.acceptedAnswer.text).toMatch(/you send it/i)
    expect(doc.text).toContain('Will Seeka get me a job?')
  })
})

describe('the guides', () => {
  it('lists every guide on the index', () => {
    expect(links(page('/guides/'))).toEqual(expect.arrayContaining(guides))
  })

  it.each(guides)('%s is an article with a date and a sign-up action', (path) => {
    const doc = page(path)
    const article = jsonLd(doc).find((item) => item['@type'] === 'Article')
    expect(article?.datePublished).toMatch(/^\d{4}-\d{2}-\d{2}/)
    expect(article?.headline).toBe(doc.querySelector('h1')?.text.trim())
    expect(doc.querySelector('time[datetime]')).not.toBeNull()
    expect(links(doc)).toContain(REGISTER)
  })
})
