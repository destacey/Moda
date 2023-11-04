'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Skeleton } from 'antd'

export default function RiskDetailsLoading() {
  return (
    <>
      <PageTitle title="Risk Details" />
      <Skeleton active />
    </>
  )
}
