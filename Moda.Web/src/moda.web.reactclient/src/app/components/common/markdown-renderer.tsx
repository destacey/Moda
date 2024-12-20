import React from 'react'
import ReactMarkdown from 'react-markdown'
import { Typography } from 'antd'
import remarkGfm from 'remark-gfm'

const { Title, Paragraph, Text } = Typography

interface MarkdownRendererProps {
  markdown?: string
}

const components = {
  h1: ({ node, ...props }) => <Title level={1} {...props} />,
  h2: ({ node, ...props }) => <Title level={2} {...props} />,
  h3: ({ node, ...props }) => <Title level={3} {...props} />,
  h4: ({ node, ...props }) => <Title level={4} {...props} />,
  h5: ({ node, ...props }) => <Title level={5} {...props} />,
  p: ({ node, ...props }) => <Paragraph {...props} />,
  strong: ({ node, ...props }) => <Text strong {...props} />,
  em: ({ node, ...props }) => <Text italic {...props} />,
}

const MarkdownRenderer = ({ markdown }: MarkdownRendererProps) => {
  if (!markdown) return null
  return (
    <div>
      <ReactMarkdown components={components} remarkPlugins={[remarkGfm]}>
        {markdown}
      </ReactMarkdown>
    </div>
  )
}

export default MarkdownRenderer
