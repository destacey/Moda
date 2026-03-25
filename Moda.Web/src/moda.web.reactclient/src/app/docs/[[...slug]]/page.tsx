import { notFound } from 'next/navigation'
import { serialize } from 'next-mdx-remote/serialize'
import { getDocBySlug, getAllDocSlugs } from '@/src/services/docs'
import DocsPageContent from '../docs-page-content'

interface DocsPageProps {
  params: Promise<{ slug?: string[] }>
}

export default async function DocsPage({ params }: DocsPageProps) {
  const { slug = [] } = await params

  // Load the root index for /docs, otherwise use the slug
  let doc = getDocBySlug(slug.length === 0 ? ['index'] : slug)

  if (!doc) {
    // Try loading as index of a directory
    doc = getDocBySlug([...slug, 'index'])
    if (!doc) {
      notFound()
    }
  }

  const mdxSource = await serialize(doc.content)

  return (
    <DocsPageContent
      title={doc.frontmatter.title}
      description={doc.frontmatter.description}
      mdxSource={mdxSource}
    />
  )
}

export async function generateStaticParams() {
  const slugs = getAllDocSlugs()
  return slugs.map((slug) => ({
    slug: slug.length === 0 ? undefined : slug,
  }))
}

export async function generateMetadata({ params }: DocsPageProps) {
  const { slug = [] } = await params
  const doc = getDocBySlug(slug.length === 0 ? ['index'] : slug)

  return {
    title: doc ? `${doc.frontmatter.title} | Moda Docs` : 'Moda Documentation',
    description: doc?.frontmatter.description,
  }
}
