'use client'

import { Button, Typography } from 'antd'
import { FC, ReactNode, useEffect, useRef, useState } from 'react'
import './expandable-content.css'

const { Text } = Typography

const LINE_HEIGHT_PX = 24

interface ExpandableContentProps {
  children: ReactNode
  lines?: number
  /** Background color for the fade gradient. Defaults to --ant-color-bg-container. */
  background?: string
}

const ExpandableContent: FC<ExpandableContentProps> = ({
  children,
  lines = 4,
  background,
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
      <div
        className="expandable-content-clamp"
        style={
          background
            ? ({ '--expandable-content-bg': background } as React.CSSProperties)
            : undefined
        }
      >
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
