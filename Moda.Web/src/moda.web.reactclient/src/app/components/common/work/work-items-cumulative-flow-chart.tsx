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
import dayjs from 'dayjs'

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
        date: dayjs(item.date).toDate(),
        category: 'Proposed',
        value: item.proposed,
      }))

      const activeData = workItems.map((item) => ({
        date: dayjs(item.date).toDate(),
        category: 'Active',
        value: item.active,
      }))

      const doneData = workItems.map((item) => ({
        date: dayjs(item.date).toDate(),
        category: 'Done',
        value: item.done,
      }))

      setData([...doneData, ...activeData, ...proposedData])
    }
  }, [props.workItems])

  const config = useMemo(() => {
    return {
      title: 'Cumulative Flow',
      theme: antDesignChartsTheme,
      data: data,
      xField: 'date',
      yField: 'value',
      seriesField: 'category',
      colorField: 'category',
      legend: {
        color: { layout: { justifyContent: 'center' }, itemMarker: 'square' },
      },
      stack: true,
      // stack: {
      //   field: 'order',
      //   reverse: false,
      //   // orderBy: (a, b) => {
      //   //   const order = ['Done', 'Active', 'Proposed']
      //   //   return order.indexOf(a) - order.indexOf(b)
      //   // },
      // },
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
