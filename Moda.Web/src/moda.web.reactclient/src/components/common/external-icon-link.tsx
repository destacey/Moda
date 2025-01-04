import { ExportOutlined } from '@ant-design/icons'
import Link from 'next/link'

export interface ExternalIconLinkProps {
  content: string | JSX.Element
  url?: string | undefined
  tooltip?: string | undefined
}

const ExternalIconLink = ({
  content,
  url,
  tooltip,
}: ExternalIconLinkProps): string | JSX.Element => {
  if (!url) return content

  return (
    <>
      {content}
      &nbsp;
      {
        <Link href={url} target="_blank" title={tooltip}>
          <ExportOutlined style={{ width: '12px' }} />
        </Link>
      }
    </>
  )
}

export default ExternalIconLink
