'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Skeleton } from 'antd'

export default function RoadmapDetailsLoading() {
  return (
    <>
      <PageTitle title="Roadmap" />
      <Skeleton active />
    </>
  )
}
