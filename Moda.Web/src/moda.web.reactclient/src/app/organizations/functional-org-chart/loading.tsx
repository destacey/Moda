'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Skeleton } from 'antd'

export default function FunctionalOrganizationChartLoading() {
  return (
    <>
      <PageTitle title="Functional Organization Chart" />
      <Skeleton active />
    </>
  )
}
