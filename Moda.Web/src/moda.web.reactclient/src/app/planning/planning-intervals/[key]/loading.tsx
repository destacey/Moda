'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Skeleton } from 'antd'

export default function PlanningIntervalDetailsLoading() {
  return (
    <>
      <PageTitle title="PI Details" />
      <Skeleton active />
    </>
  )
}
