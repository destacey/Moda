'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Skeleton } from 'antd'

export default function ProgramIncrementObjectiveDetailsLoading() {
  return (
    <>
      <PageTitle title="PI Objective Details" />
      <Skeleton active />
    </>
  )
}
