'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Skeleton } from 'antd'

export default function WorkTypesLoading() {
  return (
    <>
      <PageTitle title="Work Types" />
      <Skeleton active />
    </>
  )
}
