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
import ModaEmpty from '../moda-empty'
import { Typography } from 'antd'

const { Title } = Typography

export interface WorkItemsCumulativeFlowChartProps {
  workItems: WorkItemProgressDailyRollupDto[]
}

const WorkItemsCumulativeFlowChart = (
  props: WorkItemsCumulativeFlowChartProps,
) => {
  const [data, setData] = useState<any[]>([])
  const { antDesignChartsTheme } = useTheme()

  useMemo(() => {
    if (props.workItems) {
      const workItems = props.workItems

      const proposedData = workItems.map((item) => ({
        date: item.date,
        category: 'Proposed',
        total: item.proposed,
        color: 'gray',
      }))

      const activeData = workItems.map((item) => ({
        date: item.date,
        category: 'Active',
        total: item.active,
        color: 'blue',
      }))

      const doneData = workItems.map((item) => ({
        date: item.date,
        category: 'Done',
        total: item.done,
        color: 'green',
      }))
      setData([...doneData, ...activeData, ...proposedData])
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
      stack: true,
      //shapeField: 'smooth',
      // stack: {
      //   orderBy: 'total',
      //   reverse: true,
      // },

      // update the tooltip to show the date with this format 'MMM D, YYYY'
    } as AreaConfig
  }, [antDesignChartsTheme, data])

  if (data.length === 0)
    return (
      <>
        <Title level={5}>Cumulative Flow</Title>
        <ModaEmpty message="No work item data to display" />
      </>
    )

  return <Area {...config} />
}

export default WorkItemsCumulativeFlowChart
