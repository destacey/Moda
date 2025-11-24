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
import { Spin, Typography } from 'antd'
import dayjs from 'dayjs'

const { Title } = Typography

export interface WorkItemsCumulativeFlowChartProps {
  workItems: WorkItemProgressDailyRollupDto[]
  isLoading: boolean
}

const WorkItemsCumulativeFlowChart = (
  props: WorkItemsCumulativeFlowChartProps,
) => {
  const { antDesignChartsTheme } = useTheme()

  // Derive chart data from work items
  const data = useMemo(() => {
    if (!props.workItems) return []

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

    return [...doneData, ...activeData, ...proposedData]
  }, [props.workItems])

  const config = useMemo(() => {
    return {
      title: 'Cumulative Flow',
      theme: antDesignChartsTheme,
      data: data,
      xField: 'date',
      yField: 'value',
      //seriesField: 'category', // not sure when to use seriesField vs colorField
      colorField: 'category',
      legend: {
        color: { layout: { justifyContent: 'center' }, itemMarker: 'square' },
      },
      stack: true,
      // style: {
      //   fill: (data) => {
      //     if (data[0].category === 'Done') {
      //       return '#49aa19' // 52c41a
      //     }
      //     if (data[0].category === 'Active') {
      //       return '#1668dc' // 1677ff
      //     }
      //     if (data[0].category === 'Proposed') {
      //       return '#f5f5f5'
      //     }
      //     return '#FFC107'
      //   },
      // },
      //stackField: 'category',
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

  if (props.isLoading) return <Spin />

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
