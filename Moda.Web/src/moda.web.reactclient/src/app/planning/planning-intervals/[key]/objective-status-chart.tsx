import useTheme from '@/src/app/components/contexts/theme'
import { Pie, PieConfig } from '@ant-design/charts'
import { Card } from 'antd'
import { useMemo } from 'react'

export interface ObjectiveStatusChartProps {}

const ObjectiveStatusChart = (props: ObjectiveStatusChartProps) => {
  const { currentThemeName, antDesignChartsTheme } = useTheme()
  const fontColor =
    currentThemeName === 'light'
      ? 'rgba(0, 0, 0, 0.45)'
      : 'rgba(255, 255, 255, 0.45)'

  const config = useMemo(() => {
    return {
      title: {
        title: 'Objectives By Status',
        style: {
          titleFontSize: 14,
          titleFontWeight: 'normal',
          titleFill: fontColor,
        },
      },
      theme: antDesignChartsTheme,
      data: [
        { type: 'Not Started', count: 5 },
        { type: 'In Progress', count: 15 },
        { type: 'Completed', count: 10 },
        { type: 'Canceled', count: 3 },
        { type: 'Missed', count: 2 },
      ],
      angleField: 'count',
      colorField: 'type',
      height: 350,
      width: 450,
      label: {
        text: (d) => `${d.type}\n ${d.count}`,
        // text: (d) => `${d.type}\n ${Math.round((d.count / 35) * 100)}%`,
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
  }, [antDesignChartsTheme, fontColor])

  return (
    <Card size="small">
      <Pie {...config} />
    </Card>
  )
}

export default ObjectiveStatusChart
