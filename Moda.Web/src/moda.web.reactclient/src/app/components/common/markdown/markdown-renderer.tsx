'use client'

import ReactMarkdown from 'react-markdown'
import remarkGfm from 'remark-gfm'
import { markdownComponents } from '.'
import rehypeRaw from 'rehype-raw'

interface MarkdownRendererProps {
  markdown?: string
}

const MarkdownRenderer = ({ markdown }: MarkdownRendererProps) => {
  if (!markdown) return null
  return (
    <div style={{ width: '100%' }}>
      <ReactMarkdown
        components={markdownComponents}
        remarkPlugins={[remarkGfm]}
        rehypePlugins={[
          rehypeRaw, // To parse raw HTML
          () => (tree) => {
            // Filter out comment nodes
            tree.children = tree.children.filter(
              (node) => node.type !== 'comment',
            )
          },
        ]}
      >
        {markdown}
      </ReactMarkdown>
    </div>
  )
}

export default MarkdownRenderer
