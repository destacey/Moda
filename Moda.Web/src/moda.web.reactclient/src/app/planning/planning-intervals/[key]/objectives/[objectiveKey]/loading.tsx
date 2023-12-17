'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Skeleton } from 'antd'

export default function PlanningIntervalObjectiveDetailsLoading() {
  return (
    <>
      <PageTitle title="PI Objective Details" />
      <Skeleton active />
    </>
  )
}
