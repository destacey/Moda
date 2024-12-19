import './moda-markdown-description.css'
import MarkdownRenderer from './markdown-renderer'

export interface ModaMarkdownDescriptionProps {
  content?: string
}

const ModaMarkdownDescription = ({ content }: ModaMarkdownDescriptionProps) => {
  if (!content) return null
  return (
    <div>
      <MarkdownRenderer markdown={content} />
    </div>
  )
}

export default ModaMarkdownDescription
