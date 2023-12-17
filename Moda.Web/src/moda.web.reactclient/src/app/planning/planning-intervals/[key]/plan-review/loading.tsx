'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Skeleton } from 'antd'

export default function PlanningIntervalPlanReviewLoading() {
  return (
    <>
      <PageTitle title="PI Plan Review" />
      <Skeleton active />
    </>
  )
}
