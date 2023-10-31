'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Skeleton } from 'antd'

export default function ProgramIncrementDetailsLoading() {
  return (
    <>
      <PageTitle title="Program Increment Details" />
      <Skeleton active />
    </>
  )
}
