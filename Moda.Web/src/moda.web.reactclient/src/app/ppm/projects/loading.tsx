'use client'

import PageTitle from '@/src/components/common/page-title'
import { Skeleton } from 'antd'

export default function ProjectsLoading() {
  return (
    <>
      <PageTitle title="Projects" />
      <Skeleton active />
    </>
  )
}
