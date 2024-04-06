'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Skeleton } from 'antd'

export default function WorkspaceDetailsLoading() {
  return (
    <>
      <PageTitle title="Workspace" />
      <Skeleton active />
    </>
  )
}
