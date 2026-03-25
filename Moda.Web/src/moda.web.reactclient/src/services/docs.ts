import fs from 'fs'
import path from 'path'
import matter from 'gray-matter'

// Path to the shared docs folder.
// DOCS_PATH env var allows overriding in Docker/CI (e.g., DOCS_PATH=/docs).
// Default: 3 levels up from cwd (Moda.Web/src/moda.web.reactclient) to repo root.
const DOCS_ROOT = process.env.DOCS_PATH
  ? path.resolve(process.env.DOCS_PATH)
  : path.join(process.cwd(), '..', '..', '..', 'docs')

export interface DocFrontmatter {
  title: string
  description?: string
  sidebar_position?: number
  audience?: string[]
}

export interface DocPage {
  slug: string[]
  frontmatter: DocFrontmatter
  content: string
}

export interface DocNavItem {
  title: string
  slug: string
  children?: DocNavItem[]
  position: number
}

/**
 * Get the absolute path to a doc file from its slug segments.
 * Tries slug as a file first, then as a directory with index.
 */
function resolveDocPath(slug: string[]): string | null {
  const relativePath = slug.join('/')

  // Try exact .mdx match
  const mdxPath = path.join(DOCS_ROOT, `${relativePath}.mdx`)
  if (fs.existsSync(mdxPath)) return mdxPath

  // Try exact .md match
  const mdPath = path.join(DOCS_ROOT, `${relativePath}.md`)
  if (fs.existsSync(mdPath)) return mdPath

  // Try as directory with index
  const indexMdxPath = path.join(DOCS_ROOT, relativePath, 'index.mdx')
  if (fs.existsSync(indexMdxPath)) return indexMdxPath

  const indexMdPath = path.join(DOCS_ROOT, relativePath, 'index.md')
  if (fs.existsSync(indexMdPath)) return indexMdPath

  return null
}

/**
 * Load a single doc page by its slug segments.
 */
export function getDocBySlug(slug: string[]): DocPage | null {
  const filePath = resolveDocPath(slug)
  if (!filePath) return null

  const fileContents = fs.readFileSync(filePath, 'utf8')
  const { data, content } = matter(fileContents)

  return {
    slug,
    frontmatter: data as DocFrontmatter,
    content,
  }
}

/**
 * Get all doc slugs for static generation.
 */
export function getAllDocSlugs(): string[][] {
  const slugs: string[][] = []

  function walkDir(dir: string, parentSlug: string[] = []) {
    if (!fs.existsSync(dir)) return

    const entries = fs.readdirSync(dir, { withFileTypes: true })

    for (const entry of entries) {
      // Skip hidden files, _legacy, node_modules, ai folder (agent-only)
      if (
        entry.name.startsWith('.') ||
        entry.name.startsWith('_') ||
        entry.name === 'node_modules'
      ) {
        continue
      }

      if (entry.isDirectory()) {
        const dirSlug = [...parentSlug, entry.name]
        // If the directory has an index file, add it
        const indexPath = path.join(dir, entry.name, 'index.mdx')
        const indexMdPath = path.join(dir, entry.name, 'index.md')
        if (fs.existsSync(indexPath) || fs.existsSync(indexMdPath)) {
          slugs.push(dirSlug)
        }
        walkDir(path.join(dir, entry.name), dirSlug)
      } else if (
        (entry.name.endsWith('.mdx') || entry.name.endsWith('.md')) &&
        entry.name !== 'index.mdx' &&
        entry.name !== 'index.md'
      ) {
        const name = entry.name.replace(/\.mdx?$/, '')
        slugs.push([...parentSlug, name])
      }
    }
  }

  // Add root index
  slugs.push([])
  walkDir(DOCS_ROOT)

  return slugs
}

/**
 * Build the navigation tree for the docs sidebar.
 */
export function getDocsNavigation(): DocNavItem[] {
  function buildNav(dir: string, basePath: string): DocNavItem[] {
    if (!fs.existsSync(dir)) return []

    const entries = fs.readdirSync(dir, { withFileTypes: true })
    const items: DocNavItem[] = []

    for (const entry of entries) {
      if (
        entry.name.startsWith('.') ||
        entry.name.startsWith('_') ||
        entry.name === 'node_modules' ||
        entry.name === 'ai'
      ) {
        continue
      }

      if (entry.isDirectory()) {
        const indexPath = path.join(dir, entry.name, 'index.mdx')
        const indexMdPath = path.join(dir, entry.name, 'index.md')
        const indexFile = fs.existsSync(indexPath)
          ? indexPath
          : fs.existsSync(indexMdPath)
            ? indexMdPath
            : null

        if (indexFile) {
          const { data } = matter(fs.readFileSync(indexFile, 'utf8'))
          const slug = basePath
            ? `${basePath}/${entry.name}`
            : entry.name
          const children = buildNav(
            path.join(dir, entry.name),
            slug,
          )

          items.push({
            title: (data as DocFrontmatter).title || entry.name,
            slug,
            children: children.length > 0 ? children : undefined,
            position: (data as DocFrontmatter).sidebar_position ?? 99,
          })
        }
      } else if (
        (entry.name.endsWith('.mdx') || entry.name.endsWith('.md')) &&
        entry.name !== 'index.mdx' &&
        entry.name !== 'index.md'
      ) {
        const name = entry.name.replace(/\.mdx?$/, '')
        const filePath = path.join(dir, entry.name)
        const { data } = matter(fs.readFileSync(filePath, 'utf8'))
        const slug = basePath ? `${basePath}/${name}` : name

        items.push({
          title: (data as DocFrontmatter).title || name,
          slug,
          position: (data as DocFrontmatter).sidebar_position ?? 99,
        })
      }
    }

    return items.sort((a, b) => a.position - b.position)
  }

  return buildNav(DOCS_ROOT, '')
}
