'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Skeleton } from 'antd'

export default function WorkProcessDetailsLoading() {
  return (
    <>
      <PageTitle title="Work Process" />
      <Skeleton active />
    </>
  )
}
