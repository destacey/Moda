'use client'

import { ModaDateRange } from '@/src/components/common'
import { MarkdownRenderer } from '@/src/components/common/markdown'
import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { useGetProgramQuery } from '@/src/store/features/ppm/programs-api'
import { getDrawerWidthPixels, getSortedNames } from '@/src/utils'
import { Descriptions, Drawer, Flex } from 'antd'
import Link from 'next/link'
import { FC, useEffect, useMemo, useState } from 'react'

const { Item } = Descriptions

export interface ProgramDrawerProps {
  programKey: number
  drawerOpen: boolean
  onDrawerClose: () => void
}

const ProgramDrawer: FC<ProgramDrawerProps> = (props: ProgramDrawerProps) => {
  const [size, setSize] = useState(getDrawerWidthPixels())
  const messageApi = useMessage()

  const {
    data: programData,
    isLoading,
    error,
  } = useGetProgramQuery(props.programKey)

  const { hasPermissionClaim } = useAuth()
  const canViewProgram = useMemo(
    () => hasPermissionClaim('Permissions.Programs.View'),
    [hasPermissionClaim],
  )

  useEffect(() => {
    if (!canViewProgram) {
      messageApi.error('You do not have permission to view programs.')
      props.onDrawerClose()
    }
  }, [canViewProgram, messageApi, props])

  useEffect(() => {
    if (error) {
      messageApi.error(
        error.detail ??
          'An error occurred while loading program data. Please try again.',
      )
    }
  }, [error, messageApi])

  const sponsorNames = useMemo(
    () =>
      programData?.programSponsors.length > 0
        ? getSortedNames(programData.programSponsors)
        : 'No sponsors assigned',
    [programData],
  )

  const ownerNames = useMemo(
    () =>
      programData?.programOwners.length > 0
        ? getSortedNames(programData.programOwners)
        : 'No owners assigned',
    [programData],
  )

  const managerNames = useMemo(
    () =>
      programData?.programManagers.length > 0
        ? getSortedNames(programData.programManagers)
        : 'No managers assigned',
    [programData],
  )

  const strategicThemes = useMemo(
    () =>
      programData?.strategicThemes.length > 0
        ? getSortedNames(programData.strategicThemes)
        : null,
    [programData],
  )

  return (
    <Drawer
      title="Program Details"
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
        <Descriptions column={1} size="small">
          <Item label="Name">
            <Link href={`/ppm/programs/${programData?.key}`}>
              {programData?.name}
            </Link>
          </Item>
          <Item label="Key">{programData?.key}</Item>
          <Item label="Status">{programData?.status.name}</Item>
          <Item label="Dates">
            <ModaDateRange
              dateRange={{ start: programData?.start, end: programData?.end }}
            />
          </Item>
          <Item label="Sponsors">{sponsorNames}</Item>
          <Item label="Owners">{ownerNames}</Item>
          <Item label="Managers">{managerNames}</Item>
          <Item label="Strategic Themes">{strategicThemes}</Item>
        </Descriptions>
        <Descriptions layout="vertical" size="small">
          <Item label="Description">
            <MarkdownRenderer markdown={programData?.description} />
          </Item>
        </Descriptions>
      </Flex>
    </Drawer>
  )
}

export default ProgramDrawer
