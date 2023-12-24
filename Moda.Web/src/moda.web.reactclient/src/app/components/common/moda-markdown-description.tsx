import ReactMarkdown from 'react-markdown'
import './moda-markdown-description.css'

export interface ModaMarkdownDescriptionProps {
  content?: string
}

const ModaMarkdownDescription = ({ content }: ModaMarkdownDescriptionProps) => {
  if (!content) return null
  return (
    <div>
      <ReactMarkdown className="markdown-content">{content}</ReactMarkdown>
    </div>
  )
}

export default ModaMarkdownDescription
