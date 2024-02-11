'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Skeleton } from 'antd'

export default function UsersLoading() {
  return (
    <>
      <PageTitle title="Users" />
      <Skeleton active />
    </>
  )
}
