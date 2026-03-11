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
import { useGetProjectQuery } from '@/src/store/features/ppm/projects-api'
import { getDrawerWidthPixels, getSortedNameList } from '@/src/utils'
import { Drawer, Flex } from 'antd'
import Link from 'next/link'
import { FC, useEffect, useMemo, useState } from 'react'

export interface ProjectDrawerProps {
  projectKey: string
  drawerOpen: boolean
  onDrawerClose: () => void
}

const ProjectDrawer: FC<ProjectDrawerProps> = (props: ProjectDrawerProps) => {
  const [size, setSize] = useState(() => getDrawerWidthPixels())
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
  }, [canViewProject, messageApi, props.onDrawerClose])

  useEffect(() => {
    if (error) {
      messageApi.error(
        error.detail ??
          'An error occurred while loading project data. Please try again.',
      )
    }
  }, [error, messageApi])

  const sponsorNames = useMemo(
    () => getSortedNameList(projectData?.projectSponsors ?? []),
    [projectData],
  )

  const ownerNames = useMemo(
    () => getSortedNameList(projectData?.projectOwners ?? []),
    [projectData],
  )

  const managerNames = useMemo(
    () => getSortedNameList(projectData?.projectManagers ?? []),
    [projectData],
  )

  const memberNames = useMemo(
    () => getSortedNameList(projectData?.projectMembers ?? []),
    [projectData],
  )

  const strategicThemes = useMemo(
    () => getSortedNameList(projectData?.strategicThemes ?? []),
    [projectData],
  )

  return (
    <Drawer
      title={projectData?.name ?? 'Project Details'}
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
            <Link href={`/ppm/projects/${projectData?.key}`}>
              {projectData?.key}
            </Link>
          </LabeledContent>
          <LabeledContent label="Status">
            {projectData?.status.name}
          </LabeledContent>
          {projectData?.program && (
            <LabeledContent label="Program">
              <Link href={`/ppm/programs/${projectData.program.key}`}>
                {projectData.program.name}
              </Link>
            </LabeledContent>
          )}
          <LabeledContent label="Dates">
            <ModaDateRange
              dateRange={{ start: projectData?.start, end: projectData?.end }}
            />
          </LabeledContent>
          <LabeledContent label="Sponsors">
            <ContentList
              items={sponsorNames}
              emptyText="No sponsor assigned"
            />
          </LabeledContent>
          <LabeledContent label="Owners">
            <ContentList items={ownerNames} emptyText="No owner assigned" />
          </LabeledContent>
          <LabeledContent label="Managers">
            <ContentList
              items={managerNames}
              emptyText="No manager assigned"
            />
          </LabeledContent>
          <LabeledContent label="Members">
            <ContentList items={memberNames} emptyText="No members assigned" />
          </LabeledContent>
          {strategicThemes.length > 0 && (
            <LabeledContent label="Strategic Themes">
              {strategicThemes.join(', ')}
            </LabeledContent>
          )}
          {projectData?.description && (
            <LabeledContent label="Description">
              <ExpandableContent background="var(--ant-color-bg-elevated)">
                <MarkdownRenderer markdown={projectData.description} />
              </ExpandableContent>
            </LabeledContent>
          )}
        </Flex>
        {projectData?.id && (
          <LinksCard objectId={projectData.id} width="100%" />
        )}
      </Flex>
    </Drawer>
  )
}

export default ProjectDrawer
