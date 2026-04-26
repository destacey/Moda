'use client'

import { Tooltip, TooltipProps } from 'antd'
import {
  cloneElement,
  CSSProperties,
  isValidElement,
  ReactElement,
  ReactNode,
} from 'react'

interface WaydTooltipProps extends TooltipProps {
  helpCursor?: boolean
}

const WaydTooltip = (props: WaydTooltipProps) => {
  const { helpCursor = false, children, ...tooltipProps } = props

  if (!helpCursor) {
    return (
      <Tooltip mouseEnterDelay={0.5} {...tooltipProps}>
        {children}
      </Tooltip>
    )
  }

  const cursorStyle = { cursor: 'help' as const }

  const childWithCursor = isValidElement(children)
    ? (() => {
        const child = children as ReactElement<{ style?: CSSProperties }>
        return cloneElement(child, {
          style: {
            ...(child.props.style ?? {}),
            ...cursorStyle,
          },
        })
      })()
    : // Wrap non-element children to apply cursor without requiring callers
      // to add extra markup.
      <span style={cursorStyle}>{children as ReactNode}</span>

  return (
    <Tooltip mouseEnterDelay={0.5} {...tooltipProps}>
      {childWithCursor}
    </Tooltip>
  )
}

export default WaydTooltip
