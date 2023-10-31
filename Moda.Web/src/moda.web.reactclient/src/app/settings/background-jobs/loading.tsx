'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Skeleton } from 'antd'

export default function BackgroundJobsLoading() {
  return (
    <>
      <PageTitle title="Background Jobs" />
      <Skeleton active />
    </>
  )
}
