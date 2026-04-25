'use client'

import useTheme from '@/src/components/contexts/theme'
import dynamic from 'next/dynamic'
import type { PieConfig } from '@ant-design/charts'
import { Card } from 'antd'

const Pie = dynamic(
  () => import('@ant-design/charts').then((mod) => mod.Pie) as any,
  { ssr: false },
)

export interface ObjectiveStatusChartProps {
  data: ObjectiveStatusChartDataItem[]
}

export interface ObjectiveStatusChartDataItem {
  type: string
  count: number
}

const ObjectiveStatusChart = (props: ObjectiveStatusChartProps) => {
  const { currentThemeName, antDesignChartsTheme } = useTheme()

  if (!props.data || props.data.length === 0) return null

  const fontColor =
    currentThemeName === 'light'
      ? 'rgba(0, 0, 0, 0.45)'
      : 'rgba(255, 255, 255, 0.45)'

  const total = props.data.reduce((acc, x) => acc + x.count, 0)
  const config: PieConfig = {
    title: {
      title: 'Objectives By Status',
      style: {
        titleFontSize: 14,
        titleFontWeight: 'normal',
        titleFill: fontColor,
      },
    },
    theme: antDesignChartsTheme,
    data: props.data,
    angleField: 'count',
    colorField: 'type',
    autoFit: true,
    height: 280,
    label: {
      text: (d) =>
        `${d.type}\n ${d.count} (${Math.round((d.count / total) * 100)}%)`,
      transform: [
        {
          type: 'overlapDodgeY',
        },
      ],
    },
    legend: false,
    tooltip: {
      title: (d: ObjectiveStatusChartDataItem) => d.type,
      items: [
        {
          field: 'count',
          name: 'Objectives',
          valueFormatter: (value: number) =>
            `${value} (${Math.round((value / total) * 100)}%)`,
        },
      ],
    },
  }

  return (
    <Card size="small">
      <Pie {...(config as any)} />
    </Card>
  )
}

export default ObjectiveStatusChart
