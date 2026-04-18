import fs from 'fs'
import path from 'path'
import matter from 'gray-matter'

const DOCS_ROOT = process.env.DOCS_PATH
  ? path.resolve(process.env.DOCS_PATH)
  : path.join(process.cwd(), '..', '..', '..', 'docs')

const LINK_REGEX = /\[([^\]]*)\]\(([^)]+)\)/g

interface DocFile {
  slug: string[]
  fileSlug: string[]
  filePath: string
  title: string
  content: string
}

function findAllDocs(
  dir: string = DOCS_ROOT,
  parentSlug: string[] = [],
): DocFile[] {
  if (!fs.existsSync(dir)) return []

  const docs: DocFile[] = []
  const entries = fs.readdirSync(dir, { withFileTypes: true })

  for (const entry of entries) {
    if (
      entry.name.startsWith('.') ||
      entry.name.startsWith('_') ||
      entry.name === 'node_modules'
    ) {
      continue
    }

    const fullPath = path.join(dir, entry.name)

    if (entry.isDirectory()) {
      const indexPath = path.join(fullPath, 'index.mdx')
      const indexMdPath = path.join(fullPath, 'index.md')
      const actualIndex = fs.existsSync(indexPath)
        ? indexPath
        : fs.existsSync(indexMdPath)
          ? indexMdPath
          : null

      if (actualIndex) {
        const { data, content } = matter(fs.readFileSync(actualIndex, 'utf8'))
        const slug = [...parentSlug, entry.name]
        docs.push({
          slug,
          fileSlug: [...slug, 'index'],
          filePath: actualIndex,
          title: (data as Record<string, unknown>).title as string || entry.name,
          content,
        })
      }

      docs.push(...findAllDocs(fullPath, [...parentSlug, entry.name]))
    } else if (
      (entry.name.endsWith('.mdx') || entry.name.endsWith('.md')) &&
      !entry.name.startsWith('index.')
    ) {
      const name = entry.name.replace(/\.mdx?$/, '')
      const { data, content } = matter(fs.readFileSync(fullPath, 'utf8'))
      const slug = [...parentSlug, name]
      docs.push({
        slug,
        fileSlug: slug,
        filePath: fullPath,
        title: (data as Record<string, unknown>).title as string || name,
        content,
      })
    }
  }

  return docs
}

function docExists(targetPath: string): boolean {
  return (
    fs.existsSync(path.join(DOCS_ROOT, `${targetPath}.mdx`)) ||
    fs.existsSync(path.join(DOCS_ROOT, `${targetPath}.md`)) ||
    fs.existsSync(path.join(DOCS_ROOT, targetPath, 'index.mdx')) ||
    fs.existsSync(path.join(DOCS_ROOT, targetPath, 'index.md'))
  )
}

function extractRelativeLinks(
  content: string,
): { href: string; text: string; rawHref: string }[] {
  const links: { href: string; text: string; rawHref: string }[] = []
  LINK_REGEX.lastIndex = 0
  let match: RegExpExecArray | null

  while ((match = LINK_REGEX.exec(content)) !== null) {
    const [, text, href] = match
    if (href.startsWith('./') || href.startsWith('../')) {
      const pathOnly = href.split('#')[0].replace(/\.mdx?$/, '')
      if (pathOnly) {
        links.push({ href: pathOnly, text, rawHref: href.split('#')[0] })
      }
    }
  }

  return links
}

function resolveLink(fileSlug: string[], relativeHref: string): string {
  const sourceDir = fileSlug.slice(0, -1).join('/')
  const resolved = path.posix.normalize(path.posix.join(sourceDir, relativeHref))
  return resolved.replace(/\/index$/, '')
}

// --- Tests ---

describe('Documentation', () => {
  const allDocs = findAllDocs()

  // Add root index
  const rootIndex = path.join(DOCS_ROOT, 'index.mdx')
  if (fs.existsSync(rootIndex)) {
    const { data, content } = matter(fs.readFileSync(rootIndex, 'utf8'))
    allDocs.unshift({
      slug: [],
      fileSlug: ['index'],
      filePath: rootIndex,
      title: (data as Record<string, unknown>).title as string || 'index',
      content,
    })
  }

  it('docs root exists', () => {
    expect(fs.existsSync(DOCS_ROOT)).toBe(true)
  })

  it('found doc pages', () => {
    expect(allDocs.length).toBeGreaterThan(0)
  })

  describe('all pages have required frontmatter', () => {
    it.each(allDocs.map((d) => [d.slug.join('/') || 'index', d]))(
      '%s has title and content',
      (_, doc) => {
        const d = doc as DocFile
        expect(d.title).toBeTruthy()
        expect(d.content.length).toBeGreaterThan(0)
      },
    )
  })

  describe('no broken internal links', () => {
    const allLinks: { source: string; href: string; rawHref: string; text: string; fileSlug: string[] }[] = []

    for (const doc of allDocs) {
      const links = extractRelativeLinks(doc.content)
      for (const link of links) {
        allLinks.push({
          source: doc.slug.join('/') || 'index',
          href: link.href,
          rawHref: link.rawHref,
          text: link.text,
          fileSlug: doc.fileSlug,
        })
      }
    }

    it('found links to validate', () => {
      expect(allLinks.length).toBeGreaterThan(0)
    })

    it.each(
      allLinks.map((l) => ({
        name: `${l.source} -> "${l.href}" (${l.text})`,
        ...l,
      })),
    )('$name', ({ href, fileSlug }) => {
      const resolved = resolveLink(fileSlug, href)
      const exists = docExists(resolved)
      expect(exists).toBe(true)
    })
  })

  describe('remark plugin handles all link formats', () => {
    const linksWithExtensions: { source: string; rawHref: string; text: string; fileSlug: string[] }[] = []

    for (const doc of allDocs) {
      const links = extractRelativeLinks(doc.content)
      for (const link of links) {
        if (/\.mdx?$/.test(link.rawHref)) {
          linksWithExtensions.push({
            source: doc.slug.join('/') || 'index',
            rawHref: link.rawHref,
            text: link.text,
            fileSlug: doc.fileSlug,
          })
        }
      }
    }

    if (linksWithExtensions.length > 0) {
      it.each(
        linksWithExtensions.map((l) => ({
          name: `${l.source} -> "${l.rawHref}" (${l.text})`,
          ...l,
        })),
      )('$name targets a valid doc after extension stripping', ({ rawHref, fileSlug }) => {
        // Simulate what the remark plugin does: resolve path, strip .mdx/.md extension, strip /index
        const sourceDir = fileSlug.slice(0, -1).join('/')
        const resolved = path.posix.normalize(path.posix.join(sourceDir, rawHref))
        const cleaned = resolved.replace(/\.mdx?$/, '').replace(/\/index$/, '')
        expect(docExists(cleaned)).toBe(true)
      })
    }
  })
})
