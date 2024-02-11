'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Skeleton } from 'antd'

export default function WorkStatusesLoading() {
  return (
    <>
      <PageTitle title="Work Statuses" />
      <Skeleton active />
    </>
  )
}
