'use client'

import { ModaDateRange } from '@/src/components/common'
import { MarkdownRenderer } from '@/src/components/common/markdown'
import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { useGetStrategicInitiativeQuery } from '@/src/store/features/ppm/strategic-initiatives-api'
import { getSortedNames } from '@/src/utils'
import { getDrawerWidthPercentage } from '@/src/utils/window-utils'
import { Descriptions, Drawer, Flex, Spin } from 'antd'
import Link from 'next/link'
import { FC, useCallback, useEffect, useMemo } from 'react'

const { Item } = Descriptions

export interface StrategicInitiativeDrawerProps {
  strategicInitiativeKey: number
  drawerOpen: boolean
  onDrawerClose: () => void
}

const StrategicInitiativeDrawer: FC<StrategicInitiativeDrawerProps> = (
  props: StrategicInitiativeDrawerProps,
) => {
  const messageApi = useMessage()

  const {
    data: strategicInitiativeData,
    isLoading,
    error,
  } = useGetStrategicInitiativeQuery(props.strategicInitiativeKey)

  const { hasPermissionClaim } = useAuth()
  const canViewStrategicInitiative = useMemo(
    () => hasPermissionClaim('Permissions.StrategicInitiatives.View'),
    [hasPermissionClaim],
  )

  useEffect(() => {
    if (!canViewStrategicInitiative) {
      messageApi.error(
        'You do not have permission to view strategic initiatives.',
      )
      props.onDrawerClose()
    }
  }, [canViewStrategicInitiative, messageApi, props])

  useEffect(() => {
    if (error) {
      console.error(error)
      messageApi.error(
        error.detail ??
          'An error occurred while loading strategic initiative data. Please try again.',
      )
    }
  }, [error, messageApi])

  const sponsorNames = useMemo(
    () =>
      strategicInitiativeData?.strategicInitiativeSponsors.length > 0
        ? getSortedNames(strategicInitiativeData.strategicInitiativeSponsors)
        : 'No sponsors assigned',
    [strategicInitiativeData?.strategicInitiativeSponsors],
  )

  const ownerNames = useMemo(
    () =>
      strategicInitiativeData?.strategicInitiativeOwners.length > 0
        ? getSortedNames(strategicInitiativeData.strategicInitiativeOwners)
        : 'No owners assigned',
    [strategicInitiativeData?.strategicInitiativeOwners],
  )

  const handleDrawerClose = useCallback(() => {
    props.onDrawerClose()
  }, [props])

  return (
    <Drawer
      title="Strategic Initiative Details"
      placement="right"
      onClose={handleDrawerClose}
      open={props.drawerOpen}
      destroyOnHidden={true}
      width={getDrawerWidthPercentage()}
    >
      <Spin spinning={isLoading}>
        <Flex vertical gap="middle">
          <Descriptions column={1} size="small">
            <Item label="Name">
              <Link
                href={`/ppm/strategic-initiatives/${strategicInitiativeData?.key}`}
              >
                {strategicInitiativeData?.name}
              </Link>
            </Item>
            <Item label="Key">{strategicInitiativeData?.key}</Item>
            <Item label="Status">{strategicInitiativeData?.status.name}</Item>
            <Item label="Dates">
              <ModaDateRange
                dateRange={{
                  start: strategicInitiativeData?.start,
                  end: strategicInitiativeData?.end,
                }}
              />
            </Item>
            <Item label="Sponsors">{sponsorNames}</Item>
            <Item label="Owners">{ownerNames}</Item>
          </Descriptions>
          <Descriptions column={1} layout="vertical" size="small">
            <Item label="Description">
              <MarkdownRenderer
                markdown={strategicInitiativeData?.description}
              />
            </Item>
          </Descriptions>
        </Flex>
      </Spin>
    </Drawer>
  )
}

export default StrategicInitiativeDrawer
