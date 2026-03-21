'use client'

import { ModaEmpty } from '@/src/components/common'
import PhaseTimeline from '@/src/app/ppm/_components/phase-timeline'
import ProjectTaskMetrics from '@/src/app/ppm/projects/_components/project-task-metrics'
import { useGetProjectQuery } from '@/src/store/features/ppm/projects-api'
import { LinkOutlined } from '@ant-design/icons'
import { Button, Card, Flex, Skeleton, Tooltip, Typography } from 'antd'
import Link from 'next/link'
import { FC } from 'react'
import ProjectDetailHeader from './project-detail-header'
import ProjectPlanView from './project-plan-view'
import styles from '../my-projects-dashboard.module.css'

const { Text } = Typography

export interface ProjectDetailPanelProps {
  projectKey: string | null
}

const ProjectDetailPanel: FC<ProjectDetailPanelProps> = ({ projectKey }) => {
  if (!projectKey) {
    return (
      <div className={styles.detailEmpty}>
        <Text type="secondary">Select a project to view details</Text>
      </div>
    )
  }

  return (
    <Card size="small" key={projectKey}>
      <ProjectDetailContent projectKey={projectKey} />
    </Card>
  )
}

const ProjectDetailContent: FC<{ projectKey: string }> = ({ projectKey }) => {
  const { data: project, isLoading } = useGetProjectQuery(projectKey)

  if (isLoading) return <Skeleton active paragraph={{ rows: 8 }} />
  if (!project) {
    return (
      <div className={styles.detailEmpty}>
        <Text type="secondary">Project not found</Text>
      </div>
    )
  }

  const hasLifecycle = !!project.projectLifecycle

  return (
    <Flex vertical gap={0}>
      <ProjectDetailHeader project={project} />

      {hasLifecycle ? (
        <>
          {project.phases?.length > 0 && (
            <Flex vertical gap={8} className={styles.detailSection}>
              <Text strong style={{ fontSize: 13 }}>Phases</Text>
              <PhaseTimeline phases={project.phases} />
            </Flex>
          )}

          <Flex vertical gap={8} className={styles.detailSection}>
            <Text strong style={{ fontSize: 13 }}>Task Summary</Text>
            <ProjectTaskMetrics projectKey={project.key} />
          </Flex>

          <Flex vertical gap={8}>
            <Flex align="center" gap={4}>
              <Text strong style={{ fontSize: 13 }}>Project Plan</Text>
              <Tooltip title="Open full project plan">
                <Link href={`/ppm/projects/${project.key}#plan`}>
                  <Button type="text" size="small" icon={<LinkOutlined style={{ fontSize: 11 }} />} />
                </Link>
              </Tooltip>
            </Flex>
            <ProjectPlanView projectKey={project.key} />
          </Flex>
        </>
      ) : (
        <ModaEmpty message="No project plan defined. Assign a project lifecycle to enable planning." />
      )}
    </Flex>
  )
}

export default ProjectDetailPanel
