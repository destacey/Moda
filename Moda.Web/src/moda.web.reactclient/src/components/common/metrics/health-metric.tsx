'use client'

import { FC } from 'react'
import { MetricCard } from '.'
import useTheme from '@/src/components/contexts/theme'

export interface HealthMetricProps {
  value: number
  title: string
  tooltip?: string
  goodIfZero?: boolean
  cardStyle?: React.CSSProperties
}

const HealthMetric: FC<HealthMetricProps> = ({
  value,
  title,
  tooltip,
  goodIfZero = true,
  cardStyle,
}) => {
  const { token } = useTheme()

  let color
  if (goodIfZero) {
    color = value === 0 ? token.colorSuccess : token.colorError
  } else {
    color = value > 0 ? token.colorSuccess : token.colorError
  }

  return (
    <MetricCard
      title={title}
      value={value}
      valueStyle={{ color }}
      tooltip={tooltip}
      cardStyle={cardStyle}
    />
  )
}

export default HealthMetric
