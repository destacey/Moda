'use client'

import { Tooltip, TooltipProps } from 'antd'

const WaydTooltip = (props: TooltipProps) => (
  <Tooltip mouseEnterDelay={0.5} {...props} />
)

export default WaydTooltip
