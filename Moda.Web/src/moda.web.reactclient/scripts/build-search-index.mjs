/**
 * Build-time script that generates the docs search index as a static JSON file.
 *
 * This runs before `next build` so the search index is available as a static
 * asset in production (where the docs/ folder isn't accessible at runtime).
 *
 * Output: public/docs-search-index.json
 */
import fs from 'fs'
import path from 'path'
import matter from 'gray-matter'

const DOCS_ROOT = process.env.DOCS_PATH
  ? path.resolve(process.env.DOCS_PATH)
  : path.join(process.cwd(), '..', '..', '..', 'docs')

const OUTPUT_PATH = path.join(process.cwd(), 'public', 'docs-search-index.json')

function stripMarkdown(md) {
  let result = md
    .replace(/^---[\s\S]*?---\n?/, '')
    .replace(/```mermaid[\s\S]*?```/g, '')
    .replace(/```[\s\S]*?```/g, '')
    .replace(/`[^`]+`/g, '')
    .replace(/!\[[^\]]*\]\([^)]*\)/g, '')
    .replace(/\[([^\]]+)\]\([^)]*\)/g, '$1')
    .replace(/#{1,6}\s+/g, '')
    .replace(/\*{1,3}([^*]+)\*{1,3}/g, '$1')
    .replace(/\|/g, ' ')
    .replace(/-{3,}/g, '')

  let prev = ''
  while (prev !== result) {
    prev = result
    result = result.replace(/<[^>]*>/g, '')
  }
  result = result.replace(/[<>]/g, '')

  return result.replace(/\s+/g, ' ').trim()
}

function walkDocs(dir, parentSlug = []) {
  if (!fs.existsSync(dir)) return []

  const entries = []
  const dirEntries = fs.readdirSync(dir, { withFileTypes: true })

  for (const entry of dirEntries) {
    if (entry.name.startsWith('.') || entry.name.startsWith('_') || entry.name === 'node_modules') {
      continue
    }

    const fullPath = path.join(dir, entry.name)

    if (entry.isDirectory()) {
      const indexPath = path.join(fullPath, 'index.mdx')
      const indexMdPath = path.join(fullPath, 'index.md')
      const actualIndex = fs.existsSync(indexPath) ? indexPath : fs.existsSync(indexMdPath) ? indexMdPath : null

      if (actualIndex) {
        const { data, content } = matter(fs.readFileSync(actualIndex, 'utf8'))
        const slug = [...parentSlug, entry.name]
        entries.push({
          slug: `/docs/${slug.join('/')}`,
          title: data.title || entry.name,
          description: data.description || '',
          content: stripMarkdown(content).substring(0, 2000),
        })
      }

      entries.push(...walkDocs(fullPath, [...parentSlug, entry.name]))
    } else if ((entry.name.endsWith('.mdx') || entry.name.endsWith('.md')) && !entry.name.startsWith('index.')) {
      const name = entry.name.replace(/\.mdx?$/, '')
      const { data, content } = matter(fs.readFileSync(fullPath, 'utf8'))
      const slug = [...parentSlug, name]
      entries.push({
        slug: `/docs/${slug.join('/')}`,
        title: data.title || name,
        description: data.description || '',
        content: stripMarkdown(content).substring(0, 2000),
      })
    }
  }

  return entries
}

// Build the index
console.log(`Building docs search index from ${DOCS_ROOT}...`)

const entries = []

// Add root index
const rootIndex = path.join(DOCS_ROOT, 'index.mdx')
if (fs.existsSync(rootIndex)) {
  const { data, content } = matter(fs.readFileSync(rootIndex, 'utf8'))
  entries.push({
    slug: '/docs',
    title: data.title || 'Welcome',
    description: data.description || '',
    content: stripMarkdown(content).substring(0, 2000),
  })
}

entries.push(...walkDocs(DOCS_ROOT))

// Ensure public directory exists
const publicDir = path.dirname(OUTPUT_PATH)
if (!fs.existsSync(publicDir)) {
  fs.mkdirSync(publicDir, { recursive: true })
}

fs.writeFileSync(OUTPUT_PATH, JSON.stringify(entries, null, 2))
console.log(`Search index built: ${entries.length} entries written to ${OUTPUT_PATH}`)
