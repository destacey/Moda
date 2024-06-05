'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Skeleton } from 'antd'

export default function WorkTypeHierarchyLoading() {
  return (
    <>
      <PageTitle title="Work Type Hierarchy" />
      <Skeleton active />
    </>
  )
}
