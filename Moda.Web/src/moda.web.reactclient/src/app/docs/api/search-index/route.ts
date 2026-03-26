import { NextResponse } from 'next/server'
import { getAllDocSlugs, getDocBySlug } from '@/src/services/docs'

export interface DocSearchEntry {
  slug: string
  title: string
  description: string
  content: string
}

/**
 * Strip markdown syntax to get plain text for search indexing.
 */
function stripMarkdown(md: string): string {
  let result = md
    // Remove frontmatter (already stripped by gray-matter, but just in case)
    .replace(/^---[\s\S]*?---\n?/, '')
    // Remove mermaid code blocks entirely
    .replace(/```mermaid[\s\S]*?```/g, '')
    // Remove other code blocks
    .replace(/```[\s\S]*?```/g, '')
    // Remove inline code
    .replace(/`[^`]+`/g, '')
    // Remove images
    .replace(/!\[[^\]]*\]\([^)]*\)/g, '')
    // Remove links but keep text
    .replace(/\[([^\]]+)\]\([^)]*\)/g, '$1')
    // Remove headings markup
    .replace(/#{1,6}\s+/g, '')
    // Remove bold/italic
    .replace(/\*{1,3}([^*]+)\*{1,3}/g, '$1')
    // Remove table formatting
    .replace(/\|/g, ' ')
    .replace(/-{3,}/g, '')

  // Sanitize HTML: repeatedly strip tags until stable to prevent
  // multi-character bypass (e.g., "<scr<script>ipt>")
  let prev = ''
  while (prev !== result) {
    prev = result
    result = result.replace(/<[^>]*>/g, '')
  }
  // Remove any remaining angle brackets as a final safety pass
  result = result.replace(/[<>]/g, '')

  // Collapse whitespace
  return result.replace(/\s+/g, ' ').trim()
}

// Cache the search index in memory — docs don't change at runtime.
let cachedEntries: DocSearchEntry[] | null = null

function buildSearchIndex(): DocSearchEntry[] {
  if (cachedEntries) return cachedEntries

  const slugs = getAllDocSlugs()
  const entries: DocSearchEntry[] = []

  for (const slug of slugs) {
    const effectiveSlug = slug.length === 0 ? ['index'] : slug
    const doc = getDocBySlug(effectiveSlug) ?? getDocBySlug([...slug, 'index'])
    if (!doc) continue

    const path = slug.length === 0 ? '/docs' : `/docs/${slug.join('/')}`
    const plainContent = stripMarkdown(doc.content)

    entries.push({
      slug: path,
      title: doc.frontmatter.title || '',
      description: doc.frontmatter.description || '',
      // Limit content to first 2000 chars for search — enough for good results
      content: plainContent.substring(0, 2000),
    })
  }

  cachedEntries = entries
  return entries
}

export async function GET() {
  const entries = buildSearchIndex()
  return NextResponse.json(entries)
}
