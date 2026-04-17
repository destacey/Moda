'use client'

import React from 'react'
import ReactMarkdown from 'react-markdown'
import remarkGfm from 'remark-gfm'
import { useMarkdownComponents } from '.'
import rehypeRaw from 'rehype-raw'
import './markdown-renderer.css'

interface MarkdownRendererProps {
  markdown?: string
}

const MarkdownRenderer: React.FC<MarkdownRendererProps> = React.memo(
  ({ markdown }) => {
    const markdownComponents = useMarkdownComponents()

    if (!markdown) return null
    return (
      <div className="moda-markdown-renderer">
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
  },
)

MarkdownRenderer.displayName = 'MarkdownRenderer'

export default MarkdownRenderer
