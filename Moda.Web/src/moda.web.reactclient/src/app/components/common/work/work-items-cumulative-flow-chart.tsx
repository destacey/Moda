'use client'

import dynamic from 'next/dynamic'
const Area = dynamic(
  () => import('@ant-design/charts').then((mod) => mod.Area) as any,
  { ssr: false },
)
import { AreaConfig } from '@ant-design/charts'
import useTheme from '../../contexts/theme'
import { useMemo, useState } from 'react'

export interface WorkItemsCumulativeFlowChartProps {
  isLoading: boolean
  // minDate?: Date | null
  // maxDate?: Date | null
}

var workItemsData = [
  {
    date: '2024-06-01 00:00:00',
    total: 0,
    category: 'Scope',
  },
  {
    date: '2024-06-02 00:00:00',
    total: 0,
    category: 'Scope',
  },
  {
    date: '2024-06-03 00:00:00',
    total: 0,
    category: 'Scope',
  },
  {
    date: '2024-06-04 00:00:00',
    total: 0,
    category: 'Scope',
  },
  {
    date: '2024-06-05 00:00:00',
    total: 4,
    category: 'Scope',
  },
  {
    date: '2024-06-06 00:00:00',
    total: 4,
    category: 'Scope',
  },
  {
    date: '2024-06-07 00:00:00',
    total: 4,
    category: 'Scope',
  },
  {
    date: '2024-06-08 00:00:00',
    total: 5,
    category: 'Scope',
  },
  {
    date: '2024-06-09 00:00:00',
    total: 6,
    category: 'Scope',
  },
  {
    date: '2024-06-10 00:00:00',
    total: 6,
    category: 'Scope',
  },
  {
    date: '2024-06-11 00:00:00',
    total: 7,
    category: 'Scope',
  },
  {
    date: '2024-06-12 00:00:00',
    total: 7,
    category: 'Scope',
  },
  {
    date: '2024-06-01 00:00:00',
    total: 0,
    category: 'Done',
  },
  {
    date: '2024-06-02 00:00:00',
    total: 0,
    category: 'Done',
  },
  {
    date: '2024-06-03 00:00:00',
    total: 0,
    category: 'Done',
  },
  {
    date: '2024-06-04 00:00:00',
    total: 0,
    category: 'Done',
  },
  {
    date: '2024-06-05 00:00:00',
    total: 0,
    category: 'Done',
  },
  {
    date: '2024-06-06 00:00:00',
    total: 0,
    category: 'Done',
  },
  {
    date: '2024-06-07 00:00:00',
    total: 2,
    category: 'Done',
  },
  {
    date: '2024-06-08 00:00:00',
    total: 2,
    category: 'Done',
  },
  {
    date: '2024-06-09 00:00:00',
    total: 2,
    category: 'Done',
  },
  {
    date: '2024-06-10 00:00:00',
    total: 4,
    category: 'Done',
  },
  {
    date: '2024-06-11 00:00:00',
    total: 4,
    category: 'Done',
  },
  {
    date: '2024-06-12 00:00:00',
    total: 4,
    category: 'Done',
  },
]

const WorkItemsCumulativeFlowChart = (
  props: WorkItemsCumulativeFlowChartProps,
) => {
  const [data, setData] = useState<any[]>(workItemsData)
  const { antDesignChartsTheme } = useTheme()

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

  return <Area {...config} />
}

export default WorkItemsCumulativeFlowChart
