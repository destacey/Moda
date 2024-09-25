'use client'

import useTheme from '@/src/app/components/contexts/theme'
import { Pie, PieConfig } from '@ant-design/charts'
import { Card } from 'antd'
import { useMemo } from 'react'

export interface ObjectiveHealthChartProps {
  data: ObjectiveHealthChartDataItem[]
}

export interface ObjectiveHealthChartDataItem {
  type: string
  count: number
}

const ObjectiveHealthChart = (props: ObjectiveHealthChartProps) => {
  const { currentThemeName, antDesignChartsTheme } = useTheme()
  const fontColor =
    currentThemeName === 'light'
      ? 'rgba(0, 0, 0, 0.45)'
      : 'rgba(255, 255, 255, 0.45)'

  const config = useMemo(() => {
    return {
      title: {
        title: 'Objectives By Health',
        style: {
          titleFontSize: 14,
          titleFontWeight: 'normal',
          titleFill: fontColor,
        },
      },
      theme: antDesignChartsTheme,
      data: props.data ?? [],
      angleField: 'count',
      colorField: 'type',
      height: 350,
      width: 425,
      label: {
        text: (d) =>
          `${d.type}\n ${d.count} (${Math.round((d.count / 35) * 100)}%)`,
        //position: 'outside',
        // style: {
        //   fontWeight: 'bold',
        // },
        transform: [
          {
            type: 'overlapDodgeY',
          },
        ],
      },
      legend: {
        color: {
          title: false,
          position: 'right',
          rowPadding: 5,
        },
      },
    } as PieConfig
  }, [antDesignChartsTheme, fontColor, props.data])

  if (!props.data || props.data.length === 0) return

  return (
    <Card size="small">
      <Pie {...config} />
    </Card>
  )
}

export default ObjectiveHealthChart
