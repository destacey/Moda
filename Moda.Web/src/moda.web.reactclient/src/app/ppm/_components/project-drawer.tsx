'use client'

import { ModaDateRange } from '@/src/components/common'
import { MarkdownRenderer } from '@/src/components/common/markdown'
import useAuth from '@/src/components/contexts/auth'
import { useGetProjectQuery } from '@/src/store/features/ppm/projects-api'
import { getSortedNames } from '@/src/utils'
import { getDrawerWidthPercentage } from '@/src/utils/window-utils'
import { Descriptions, Drawer, Flex, Spin } from 'antd'
import { MessageInstance } from 'antd/es/message/interface'
import Link from 'next/link'
import { useCallback, useEffect, useMemo } from 'react'

const { Item } = Descriptions

export interface ProjectDrawerProps {
  projectKey: number
  drawerOpen: boolean
  onDrawerClose: () => void
  messageApi: MessageInstance
}

const ProjectDrawer: React.FC<ProjectDrawerProps> = (
  props: ProjectDrawerProps,
) => {
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
      props.messageApi.error('You do not have permission to view projects.')
      props.onDrawerClose()
    }
  }, [canViewProject, props])

  useEffect(() => {
    if (error) {
      console.error(error)
      props.messageApi.error(
        error.detail ??
          'An error occurred while loading project data. Please try again.',
      )
    }
  }, [error, props.messageApi])

  const sponsorNames = useMemo(
    () =>
      projectData?.projectSponsors.length > 0
        ? getSortedNames(projectData.projectSponsors)
        : 'No sponsors assigned',
    [projectData?.projectSponsors],
  )

  const ownerNames = useMemo(
    () =>
      projectData?.projectOwners.length > 0
        ? getSortedNames(projectData.projectOwners)
        : 'No owners assigned',
    [projectData?.projectOwners],
  )

  const managerNames = useMemo(
    () =>
      projectData?.projectManagers.length > 0
        ? getSortedNames(projectData.projectManagers)
        : 'No managers assigned',
    [projectData?.projectManagers],
  )

  const strategicThemes = useMemo(
    () =>
      projectData?.strategicThemes.length > 0
        ? getSortedNames(projectData.strategicThemes)
        : null,
    [projectData?.strategicThemes],
  )

  const handleDrawerClose = useCallback(() => {
    props.onDrawerClose()
  }, [props])

  return (
    <Drawer
      title="Project Details"
      placement="right"
      onClose={handleDrawerClose}
      open={props.drawerOpen}
      destroyOnClose={true}
      width={getDrawerWidthPercentage()}
    >
      <Spin spinning={isLoading}>
        <Flex vertical gap="middle">
          <Descriptions column={1} size="small">
            <Item label="Name">
              <Link href={`/ppm/projects/${projectData?.key}`}>
                {projectData?.name}
              </Link>
            </Item>
            <Item label="Key">{projectData?.key}</Item>
            <Item label="Status">{projectData?.status.name}</Item>
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
      </Spin>
    </Drawer>
  )
}

export default ProjectDrawer
