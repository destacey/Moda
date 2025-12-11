'use client'

import { ModaDateRange } from '@/src/components/common'
import { MarkdownRenderer } from '@/src/components/common/markdown'
import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { useGetProjectQuery } from '@/src/store/features/ppm/projects-api'
import { getSortedNames } from '@/src/utils'
import { Descriptions, Drawer, Flex, Spin } from 'antd'
import Link from 'next/link'
import { FC, useCallback, useEffect, useMemo } from 'react'

const { Item } = Descriptions

export interface ProjectDrawerProps {
  projectKey: number
  drawerOpen: boolean
  onDrawerClose: () => void
}

const ProjectDrawer: FC<ProjectDrawerProps> = (props: ProjectDrawerProps) => {
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
      console.error(error)
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

  const handleDrawerClose = useCallback(() => {
    props.onDrawerClose()
  }, [props])

  return (
    <Drawer
      title="Project Details"
      placement="right"
      onClose={handleDrawerClose}
      open={props.drawerOpen}
      destroyOnHidden={true}
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
      </Spin>
    </Drawer>
  )
}

export default ProjectDrawer
