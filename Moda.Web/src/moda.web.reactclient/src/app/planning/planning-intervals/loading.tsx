'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Skeleton } from 'antd'

export default function PlanningIntervalsLoading() {
  return (
    <>
      <PageTitle title="Planning Intervals" />
      <Skeleton active />
    </>
  )
}
