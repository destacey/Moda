'use client'

import { Button, Typography } from 'antd'
import { FC, ReactNode, useEffect, useRef, useState } from 'react'
import './expandable-content.css'

const { Text } = Typography

const LINE_HEIGHT_PX = 24

interface ExpandableContentProps {
  children: ReactNode
  lines?: number
}

const ExpandableContent: FC<ExpandableContentProps> = ({
  children,
  lines = 4,
}) => {
  const [expanded, setExpanded] = useState(false)
  const [overflows, setOverflows] = useState(false)
  const contentRef = useRef<HTMLDivElement>(null)
  const maxHeight = LINE_HEIGHT_PX * lines

  useEffect(() => {
    if (contentRef.current) {
      setOverflows(contentRef.current.scrollHeight > maxHeight)
    }
  }, [children, maxHeight])

  const isClamped = overflows && !expanded

  return (
    <div>
      <div className="expandable-content-clamp">
        <div
          ref={contentRef}
          style={{
            maxHeight: expanded ? undefined : maxHeight,
            overflow: 'hidden',
          }}
        >
          {children}
        </div>
        {isClamped && <div className="expandable-content-fade" />}
      </div>
      {overflows && (
        <Button
          type="link"
          size="small"
          style={{ padding: 0, height: 'auto' }}
          onClick={() => setExpanded((v) => !v)}
        >
          <Text style={{ fontSize: 12 }}>{expanded ? 'Show less' : 'Show more'}</Text>
        </Button>
      )}
    </div>
  )
}

export default ExpandableContent
