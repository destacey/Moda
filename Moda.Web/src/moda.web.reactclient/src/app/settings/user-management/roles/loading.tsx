'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Skeleton } from 'antd'

export default function RolesLoading() {
  return (
    <>
      <PageTitle title="Roles" />
      <Skeleton active />
    </>
  )
}
