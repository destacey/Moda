'use client'

import { MDXRemote, MDXRemoteSerializeResult } from 'next-mdx-remote'
import { Typography } from 'antd'
import MermaidDiagram from './mermaid-diagram'

const { Title, Paragraph } = Typography

// Custom component to intercept mermaid code blocks
const mdxComponents = {
  pre: ({ children, ...props }: React.HTMLAttributes<HTMLPreElement>) => {
    // Check if the child is a <code> with mermaid language
    const child = children as React.ReactElement<{
      className?: string
      children?: string
    }>
    if (child?.props?.className === 'language-mermaid') {
      return <MermaidDiagram chart={String(child.props.children).trim()} />
    }
    return <pre {...props}>{children}</pre>
  },
}

interface DocsPageContentProps {
  title: string
  description?: string
  mdxSource: MDXRemoteSerializeResult
}

export default function DocsPageContent({
  title,
  description,
  mdxSource,
}: DocsPageContentProps) {
  return (
    <>
      <Title>{title}</Title>
      {description && (
        <Paragraph type="secondary">{description}</Paragraph>
      )}
      <div className="docs-content">
        <MDXRemote {...mdxSource} components={mdxComponents} />
      </div>
    </>
  )
}
