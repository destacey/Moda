'use client'

import { Space, Tag, Tooltip } from 'antd'

interface WorkItemTagsCellProps {
  tags: string[] | undefined
  maxVisible?: number
}

const WorkItemTagsCell = ({ tags, maxVisible }: WorkItemTagsCellProps) => {
  if (!tags || tags.length === 0) return null

  const visible = maxVisible !== undefined ? tags.slice(0, maxVisible) : tags
  const hidden = maxVisible !== undefined ? tags.slice(maxVisible) : []

  return (
    <Space
      wrap={false}
      size={[2, 0]}
      style={{ overflow: 'hidden', flexWrap: 'nowrap' }}
    >
      {visible.map((t) => (
        <Tag key={t} style={{ margin: 0 }}>
          {t}
        </Tag>
      ))}
      {hidden.length > 0 && (
        <Tooltip title={hidden.join(', ')}>
          <Tag style={{ margin: 0 }}>+{hidden.length}</Tag>
        </Tooltip>
      )}
    </Space>
  )
}

export default WorkItemTagsCell

