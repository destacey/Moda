'use client'

import dynamic from 'next/dynamic'
const Area = dynamic(
  () => import('@ant-design/charts').then((mod) => mod.Area) as any,
  { ssr: false },
)
import { AreaConfig } from '@ant-design/charts'
import useTheme from '../../contexts/theme'
import { useMemo, useState } from 'react'
import { WorkItemProgressDailyRollupDto } from '@/src/services/moda-api'

export interface WorkItemsCumulativeFlowChartProps {
  workItems: WorkItemProgressDailyRollupDto[]
}

const WorkItemsCumulativeFlowChart = (
  props: WorkItemsCumulativeFlowChartProps,
) => {
  const [data, setData] = useState<any[]>()
  const { antDesignChartsTheme } = useTheme()

  useMemo(() => {
    if (props.workItems) {
      const workItems = props.workItems
      const scopeData = workItems.map((item) => ({
        date: item.date,
        category: 'Scope',
        total: item.total,
      }))

      const doneData = workItems.map((item) => ({
        date: item.date,
        category: 'Done',
        total: item.done,
      }))
      setData([...scopeData, ...doneData])
    }
  }, [props.workItems])

  const config = useMemo(() => {
    return {
      title: 'Cumulative Flow',
      theme: antDesignChartsTheme,
      data: data,
      xField: (d) => new Date(d.date),
      yField: 'total',
      seriesField: 'category',
      colorField: 'category',
      legend: {
        color: { layout: { justifyContent: 'center' }, itemMarker: 'square' },
      },
      //shapeField: 'smooth',
      // stack: {
      //   orderBy: 'total',
      //   reverse: true,
      // },

      // update the tooltip to show the date with this format 'MMM D, YYYY'
    } as AreaConfig
  }, [antDesignChartsTheme, data])

  if (!props.workItems || props.workItems?.length === 0)
    return <div>No data to display</div>

  return <Area {...config} />
}

export default WorkItemsCumulativeFlowChart
