'use client'

import { Grid } from 'antd'

const { useBreakpoint } = Grid

// Dev-only floating chip that shows the current Ant Design breakpoint and
// viewport width. Mount from the root layout behind a NODE_ENV check so it
// is tree-shaken out of production builds.
const BreakpointIndicator = () => {
  const screens = useBreakpoint()

  // Pick the largest matched breakpoint — Ant Design returns all matches.
  const order = ['xxl', 'xl', 'lg', 'md', 'sm', 'xs'] as const
  const active = order.find((b) => screens[b]) ?? 'xs'

  return (
    <div
      style={{
        position: 'fixed',
        bottom: 12,
        right: 12,
        zIndex: 9999,
        padding: '4px 10px',
        borderRadius: 6,
        fontFamily:
          'ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace',
        fontSize: 12,
        lineHeight: 1.4,
        color: '#fff',
        background: 'rgba(0, 0, 0, 0.75)',
        pointerEvents: 'none',
        userSelect: 'none',
      }}
      aria-hidden
    >
      {active.toUpperCase()}
    </div>
  )
}

export default BreakpointIndicator
