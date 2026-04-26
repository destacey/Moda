'use client'

import { ChartCard } from '@/src/components/common/metrics'
import useTheme from '@/src/components/contexts/theme'
import { PlanningIntervalObjectiveListDto } from '@/src/services/wayd-api'
import dynamic from 'next/dynamic'
import type { PieConfig } from '@ant-design/charts'

const Pie = dynamic(
  () => import('@ant-design/charts').then((mod) => mod.Pie) as any,
  { ssr: false },
)

export interface ObjectiveStatusChartProps {
  objectivesData: PlanningIntervalObjectiveListDto[]
  embedded?: boolean
  height?: number
}

interface ObjectiveStatusChartDataItem {
  type: string
  count: number
}

const ObjectiveStatusChart = (props: ObjectiveStatusChartProps) => {
  const { antDesignChartsTheme } = useTheme()

  if (!props.objectivesData || props.objectivesData.length === 0) return null

  const groupedStatusData = props.objectivesData.reduce(
    (acc, objective) => {
      const status = objective.status?.name ?? 'Unknown'
      acc[status] = acc[status] ? acc[status] + 1 : 1
      return acc
    },
    {} as Record<string, number>,
  )

  const data: ObjectiveStatusChartDataItem[] = Object.entries(
    groupedStatusData,
  ).map(([status, count]) => ({ type: status, count }))

  const total = data.reduce((acc, x) => acc + x.count, 0)
  const config: PieConfig = {
    theme: antDesignChartsTheme,
    data,
    angleField: 'count',
    colorField: 'type',
    autoFit: true,
    height: props.height ?? 280,
    padding: props.embedded ? 0 : 'auto',
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
    <ChartCard title="Objectives By Status" embedded={props.embedded}>
      <Pie {...(config as any)} />
    </ChartCard>
  )
}

export default ObjectiveStatusChart
