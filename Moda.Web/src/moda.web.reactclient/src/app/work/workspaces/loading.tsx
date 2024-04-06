'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Skeleton } from 'antd'

export default function WorkspacesLoading() {
  return (
    <>
      <PageTitle title="Workspaces" />
      <Skeleton active />
    </>
  )
}
