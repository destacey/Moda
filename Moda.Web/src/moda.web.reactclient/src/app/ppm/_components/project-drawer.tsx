'use client'

import { ModaDateRange } from '@/src/components/common'
import { MarkdownRenderer } from '@/src/components/common/markdown'
import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { useGetProjectQuery } from '@/src/store/features/ppm/projects-api'
import { getDrawerWidthPixels, getSortedNames } from '@/src/utils'
import { Descriptions, Drawer, Flex } from 'antd'
import Link from 'next/link'
import { FC, useEffect, useMemo, useState } from 'react'

const { Item } = Descriptions

export interface ProjectDrawerProps {
  projectKey: string
  drawerOpen: boolean
  onDrawerClose: () => void
}

const ProjectDrawer: FC<ProjectDrawerProps> = (props: ProjectDrawerProps) => {
  const [size, setSize] = useState(getDrawerWidthPixels())
  const messageApi = useMessage()

  const {
    data: projectData,
    isLoading,
    error,
  } = useGetProjectQuery(props.projectKey)

  const { hasPermissionClaim } = useAuth()
  const canViewProject = useMemo(
    () => hasPermissionClaim('Permissions.Projects.View'),
    [hasPermissionClaim],
  )

  useEffect(() => {
    if (!canViewProject) {
      messageApi.error('You do not have permission to view projects.')
      props.onDrawerClose()
    }
  }, [canViewProject, messageApi, props])

  useEffect(() => {
    if (error) {
      messageApi.error(
        error.detail ??
          'An error occurred while loading project data. Please try again.',
      )
    }
  }, [error, messageApi])

  const sponsorNames = useMemo(
    () =>
      projectData?.projectSponsors.length > 0
        ? getSortedNames(projectData.projectSponsors)
        : 'No sponsors assigned',
    [projectData],
  )

  const ownerNames = useMemo(
    () =>
      projectData?.projectOwners.length > 0
        ? getSortedNames(projectData.projectOwners)
        : 'No owners assigned',
    [projectData],
  )

  const managerNames = useMemo(
    () =>
      projectData?.projectManagers.length > 0
        ? getSortedNames(projectData.projectManagers)
        : 'No managers assigned',
    [projectData],
  )

  const strategicThemes = useMemo(
    () =>
      projectData?.strategicThemes.length > 0
        ? getSortedNames(projectData.strategicThemes)
        : null,
    [projectData],
  )

  return (
    <Drawer
      title="Project Details"
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
            <Link href={`/ppm/projects/${projectData?.key}`}>
              {projectData?.name}
            </Link>
          </Item>
          <Item label="Key">{projectData?.key}</Item>
          <Item label="Status">{projectData?.status.name}</Item>

          {projectData?.program && (
            <Item label="Program">
              <Link href={`/ppm/programs/${projectData.program.key}`}>
                {projectData.program.name}
              </Link>
            </Item>
          )}
          <Item label="Dates">
            <ModaDateRange
              dateRange={{ start: projectData?.start, end: projectData?.end }}
            />
          </Item>
          <Item label="Sponsors">{sponsorNames}</Item>
          <Item label="Owners">{ownerNames}</Item>
          <Item label="Managers">{managerNames}</Item>
          <Item label="Strategic Themes">{strategicThemes}</Item>
        </Descriptions>
        <Descriptions layout="vertical" size="small">
          <Item label="Description">
            <MarkdownRenderer markdown={projectData?.description} />
          </Item>
        </Descriptions>
      </Flex>
    </Drawer>
  )
}

export default ProjectDrawer
