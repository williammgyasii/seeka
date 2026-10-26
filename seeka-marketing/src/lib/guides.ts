import { getCollection } from 'astro:content'

const entries = (await getCollection('guides')).sort((a, b) => a.data.order - b.data.order)

export const guides = entries.map((entry) => ({
  entry,
  path: `/guides/${entry.id}/`,
  title: entry.data.title,
  summary: entry.data.summary,
}))
