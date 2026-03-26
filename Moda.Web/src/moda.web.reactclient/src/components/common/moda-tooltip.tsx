'use client'

import { Tooltip, TooltipProps } from 'antd'

const ModaTooltip = (props: TooltipProps) => (
  <Tooltip mouseEnterDelay={0.5} {...props} />
)

export default ModaTooltip
