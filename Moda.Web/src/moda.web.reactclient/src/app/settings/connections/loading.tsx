'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Skeleton } from 'antd'

export default function ConnectionsLoading() {
  return (
    <>
      <PageTitle title="Connections" />
      <Skeleton active />
    </>
  )
}
