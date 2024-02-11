'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Skeleton } from 'antd'

export default function SettingsLoading() {
  return (
    <>
      <PageTitle title="Settings" />
      <Skeleton active />
    </>
  )
}
