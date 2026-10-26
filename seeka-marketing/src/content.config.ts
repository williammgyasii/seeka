import { defineCollection } from 'astro:content'
import { glob } from 'astro/loaders'
import { z } from 'astro/zod'

const guides = defineCollection({
  loader: glob({ pattern: '*.md', base: './src/content/guides' }),
  schema: z.object({
    title: z.string(),
    description: z.string().min(70).max(170),
    summary: z.string(),
    published: z.coerce.date(),
    order: z.number(),
  }),
})

export const collections = { guides }
