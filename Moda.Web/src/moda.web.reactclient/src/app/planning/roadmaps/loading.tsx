'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Skeleton } from 'antd'

export default function RoadmapsLoading() {
  return (
    <>
      <PageTitle title="Roadmaps" />
      <Skeleton active />
    </>
  )
}
