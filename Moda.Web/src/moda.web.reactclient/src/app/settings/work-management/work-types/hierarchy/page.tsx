'use client'

import { PageTitle } from '@/src/app/components/common'
import BasicBreadcrumb from '@/src/app/components/common/basic-breadcrumb'
import useAuth from '@/src/app/components/contexts/auth'
import { authorizePage } from '@/src/app/components/hoc'
import { useDocumentTitle } from '@/src/app/hooks'
import { useGetWorkTypeLevelsQuery } from '@/src/store/features/work-management/work-type-level-api'
import { useGetWorkTypeTiersQuery } from '@/src/store/features/work-management/work-type-tier-api'
import { Space, Spin } from 'antd'
import { WorkTypeTierCard } from '../components'

const HierarchyPage = () => {
  useDocumentTitle('Work Management - Work Type Hierarchy')

  const { data: workTiers, isLoading: workTiersIsLoading } =
    useGetWorkTypeTiersQuery(null)
  const {
    data: workLevels,
    isLoading: workLevelsIsLoading,
    refetch: refetchLevels,
  } = useGetWorkTypeLevelsQuery(null)

  const { hasClaim } = useAuth()
  const canCreateWorkTypeLevels = hasClaim(
    'Permission',
    'Permissions.WorkTypeLevels.Create',
  )
  const canUpdateWorkTypeLevels = hasClaim(
    'Permission',
    'Permissions.WorkTypeLevels.Update',
  )

  return (
    <>
      <BasicBreadcrumb
        items={[
          { title: 'Settings' },
          { title: 'Work Management' },
          { title: 'Work Types', href: './' },
          { title: 'Hierarchy' },
        ]}
      />
      <PageTitle title="Work Type Hierarchy" />

      <Spin
        spinning={workTiersIsLoading || workLevelsIsLoading}
        tip="Loading work type tiers and levels..."
        size="large"
        style={{ paddingTop: 50 }}
      >
        <Space direction="vertical">
          {workTiers?.map((tier) => (
            <WorkTypeTierCard
              key={tier.id}
              tier={tier}
              levels={workLevels?.filter((level) => level.tier.id === tier.id)}
              refreshLevels={refetchLevels}
              canCreateWorkTypeLevels={canCreateWorkTypeLevels}
              canUpdateWorkTypeLevels={canUpdateWorkTypeLevels}
            />
          ))}
        </Space>
      </Spin>
    </>
  )
}

const HierarchyPageWithAuthorization = authorizePage(
  HierarchyPage,
  'Permission',
  'Permissions.WorkTypeLevels.View',
)

export default HierarchyPageWithAuthorization
