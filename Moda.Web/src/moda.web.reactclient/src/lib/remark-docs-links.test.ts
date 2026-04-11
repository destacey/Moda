// Mock unist-util-visit since it's ESM-only and next/jest doesn't transform it
jest.mock('unist-util-visit', () => ({
  visit: (tree: { children: { type: string; url?: string }[] }, type: string, fn: (node: { type: string; url?: string }) => void) => {
    for (const node of tree.children) {
      if (node.type === type) fn(node)
    }
  },
}))

import remarkDocsLinks from './remark-docs-links'

// Create a minimal mock AST link node and tree to test the plugin
// without needing the full unified/remark pipeline (which is ESM-only)
interface LinkNode {
  type: 'link'
  url: string
  children: { type: 'text'; value: string }[]
}

interface TreeNode {
  type: string
  children: (TreeNode | LinkNode)[]
}

function createLinkNode(url: string): LinkNode {
  return { type: 'link', url, children: [{ type: 'text', value: 'link' }] }
}

function createTree(links: LinkNode[]): TreeNode {
  return { type: 'root', children: links }
}

function transformLinks(
  urls: string[],
  slug: string[],
  basePath = '/docs',
): string[] {
  const links = urls.map(createLinkNode)
  const tree = createTree(links)
  const plugin = remarkDocsLinks({ slug, basePath })
  plugin(tree)
  return links.map((l) => l.url)
}

describe('remark-docs-links', () => {
  describe('resolves relative links to absolute paths', () => {
    it('resolves ./ link from index page', () => {
      const [url] = transformLinks(
        ['./projects'],
        ['user-guide', 'ppm', 'index'],
      )
      expect(url).toBe('/docs/user-guide/ppm/projects')
    })

    it('resolves ../ link from leaf page', () => {
      const [url] = transformLinks(
        ['../organizations/index#teams'],
        ['user-guide', 'planning', 'sprints'],
      )
      expect(url).toBe('/docs/user-guide/organizations#teams')
    })

    it('resolves ./ link from leaf page', () => {
      const [url] = transformLinks(
        ['./work-configuration#work-types'],
        ['user-guide', 'work-management', 'work-items'],
      )
      expect(url).toBe(
        '/docs/user-guide/work-management/work-configuration#work-types',
      )
    })
  })

  describe('strips .mdx extensions from links', () => {
    it('strips .mdx from ./ link', () => {
      const [url] = transformLinks(
        ['./projects.mdx'],
        ['user-guide', 'ppm', 'index'],
      )
      expect(url).toBe('/docs/user-guide/ppm/projects')
    })

    it('strips .mdx from ../ link', () => {
      const [url] = transformLinks(
        ['../organizations/index.mdx#teams'],
        ['user-guide', 'planning', 'sprints'],
      )
      expect(url).toBe('/docs/user-guide/organizations#teams')
    })

    it('strips .md extension', () => {
      const [url] = transformLinks(
        ['./guide.md'],
        ['docs', 'index'],
      )
      expect(url).toBe('/docs/docs/guide')
    })

    it('does not produce URLs ending in .mdx', () => {
      const [url] = transformLinks(
        ['./portfolios-programs.mdx'],
        ['user-guide', 'ppm', 'index'],
      )
      expect(url).not.toMatch(/\.mdx?($|#)/)
    })
  })

  describe('strips trailing /index', () => {
    it('removes /index from resolved path', () => {
      const [url] = transformLinks(
        ['./planning/index'],
        ['user-guide', 'index'],
      )
      expect(url).toBe('/docs/user-guide/planning')
    })

    it('removes /index and strips .mdx', () => {
      const [url] = transformLinks(
        ['./planning/index.mdx'],
        ['user-guide', 'index'],
      )
      expect(url).toBe('/docs/user-guide/planning')
    })
  })

  describe('preserves anchors', () => {
    it('keeps hash fragment after stripping extension', () => {
      const [url] = transformLinks(
        ['./work-configuration.mdx#work-types'],
        ['user-guide', 'work-management', 'work-items'],
      )
      expect(url).toBe(
        '/docs/user-guide/work-management/work-configuration#work-types',
      )
    })
  })

  describe('skips non-relative links', () => {
    it('does not modify external links', () => {
      const [url] = transformLinks(
        ['https://github.com'],
        ['user-guide', 'index'],
      )
      expect(url).toBe('https://github.com')
    })

    it('does not modify absolute links', () => {
      const [url] = transformLinks(
        ['/docs/user-guide'],
        ['user-guide', 'index'],
      )
      expect(url).toBe('/docs/user-guide')
    })

    it('does not modify anchor-only links', () => {
      const [url] = transformLinks(
        ['#my-section'],
        ['user-guide', 'index'],
      )
      expect(url).toBe('#my-section')
    })
  })
})
