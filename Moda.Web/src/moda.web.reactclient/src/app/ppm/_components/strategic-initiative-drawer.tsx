'use client'

import { ModaDateRange } from '@/src/components/common'
import {
  ContentList,
  ExpandableContent,
  LabeledContent,
} from '@/src/components/common/content'
import LinksCard from '@/src/components/common/links/links-card'
import { MarkdownRenderer } from '@/src/components/common/markdown'
import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { useGetStrategicInitiativeQuery } from '@/src/store/features/ppm/strategic-initiatives-api'
import { getDrawerWidthPixels, getSortedNameList } from '@/src/utils'
import { Drawer, Flex } from 'antd'
import Link from 'next/link'
import { FC, useEffect, useMemo, useState } from 'react'

export interface StrategicInitiativeDrawerProps {
  strategicInitiativeKey: number
  drawerOpen: boolean
  onDrawerClose: () => void
}

const StrategicInitiativeDrawer: FC<StrategicInitiativeDrawerProps> = (
  props: StrategicInitiativeDrawerProps,
) => {
  const [size, setSize] = useState(() => getDrawerWidthPixels())
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
  }, [canViewStrategicInitiative, messageApi, props.onDrawerClose])

  useEffect(() => {
    if (error) {
      messageApi.error(
        error.detail ??
          'An error occurred while loading strategic initiative data. Please try again.',
      )
    }
  }, [error, messageApi])

  const sponsorNames = useMemo(
    () =>
      getSortedNameList(
        strategicInitiativeData?.strategicInitiativeSponsors ?? [],
      ),
    [strategicInitiativeData],
  )

  const ownerNames = useMemo(
    () =>
      getSortedNameList(
        strategicInitiativeData?.strategicInitiativeOwners ?? [],
      ),
    [strategicInitiativeData],
  )

  return (
    <Drawer
      title={strategicInitiativeData?.name ?? 'Strategic Initiative Details'}
      placement="right"
      onClose={props.onDrawerClose}
      open={props.drawerOpen}
      loading={isLoading}
      size={size}
      resizable={{
        onResize: (newSize) => setSize(newSize),
      }}
      destroyOnHidden={true}
    >
      <Flex vertical gap="middle">
        <Flex vertical gap={10}>
          <LabeledContent label="Key">
            <Link
              href={`/ppm/strategic-initiatives/${strategicInitiativeData?.key}`}
            >
              {strategicInitiativeData?.key}
            </Link>
          </LabeledContent>
          <LabeledContent label="Status">
            {strategicInitiativeData?.status.name}
          </LabeledContent>
          <LabeledContent label="Dates">
            <ModaDateRange
              dateRange={{
                start: strategicInitiativeData?.start,
                end: strategicInitiativeData?.end,
              }}
            />
          </LabeledContent>
          <LabeledContent label="Sponsors">
            <ContentList
              items={sponsorNames}
              emptyText="No sponsors assigned"
            />
          </LabeledContent>
          <LabeledContent label="Owners">
            <ContentList items={ownerNames} emptyText="No owners assigned" />
          </LabeledContent>
          {strategicInitiativeData?.description && (
            <LabeledContent label="Description">
              <ExpandableContent background="var(--ant-color-bg-elevated)">
                <MarkdownRenderer
                  markdown={strategicInitiativeData.description}
                />
              </ExpandableContent>
            </LabeledContent>
          )}
        </Flex>
        {strategicInitiativeData?.id && (
          <LinksCard objectId={strategicInitiativeData.id} width="100%" />
        )}
      </Flex>
    </Drawer>
  )
}

export default StrategicInitiativeDrawer
