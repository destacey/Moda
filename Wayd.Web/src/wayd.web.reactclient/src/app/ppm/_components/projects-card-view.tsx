'use client'

import { LifecycleStatusTag } from '@/src/components/common'
import TimelineProgress from '@/src/components/common/planning/timeline-progress'
import PhaseTimeline from './phase-timeline'
import { ProjectListDto } from '@/src/services/wayd-api'
import { getSortedNames } from '@/src/utils'
import { Card, Flex, Spin, Typography } from 'antd'
import styles from './projects-card-view.module.css'
import Link from 'next/link'
import { FC, ReactNode } from 'react'
import { ProjectHealthCheckTag } from '../projects/_components'

const { Text } = Typography

interface ProjectCardProps {
  project: ProjectListDto
  onCardClick: (key: string) => void
  hidePortfolio?: boolean
}

const ProjectCard: FC<ProjectCardProps> = ({ project, onCardClick, hidePortfolio }) => {
  const managerNames = getSortedNames(project.projectManagers)

  const timelineFormat =
    project.start &&
    project.end &&
    new Date(project.start).getFullYear() === new Date().getFullYear()
      ? 'MMM D'
      : 'MMM D, YYYY'

  return (
    <Card
      size="small"
      hoverable
      className={styles.card}
      onClick={() => onCardClick(project.key)}
    >
      <Flex vertical gap={1} style={{ flex: 1 }}>
        {/* Header */}
        <Flex justify="space-between" gap="small">
          <Text type="secondary" style={{ fontSize: 11 }}>
            {project.key}
          </Text>

          <Flex align="center" gap="small">
            <ProjectHealthCheckTag
              healthCheck={project.healthCheck}
              projectId={project.id}
              variant="flag"
            />
            <LifecycleStatusTag status={project.status} />
          </Flex>
        </Flex>

        <Link
          href={`/ppm/projects/${project.key}`}
          onClick={(e) => e.stopPropagation()}
          style={{ width: 'fit-content' }}
        >
          {project.name}
        </Link>

        {/* Meta rows */}
        <Flex vertical gap={3}>
          {!hidePortfolio && (
            <Flex gap={6} align="center">
              <Text type="secondary" style={{ fontSize: 11, minWidth: 60 }}>
                Portfolio
              </Text>
              <Text
                style={{ fontSize: 12 }}
                ellipsis={{ tooltip: project.portfolio.name }}
              >
                {project.portfolio.name}
              </Text>
            </Flex>
          )}
          <Flex gap={6} align="center">
            <Text type="secondary" style={{ fontSize: 11, minWidth: 60 }}>
              Managers
            </Text>
            <Text style={{ fontSize: 12 }} ellipsis={{ tooltip: managerNames }}>
              {managerNames || 'No manager assigned'}
            </Text>
          </Flex>
        </Flex>

        {/* Phases */}
        {project.phases?.length > 0 ? (
          <PhaseTimeline phases={project.phases} displayMode="small" />
        ) : (
          <Text type="secondary" style={{ fontSize: 12 }}>
            No lifecycle defined
          </Text>
        )}

        {/* Timeline */}
        {project.start && project.end && (
          <TimelineProgress
            start={project.start}
            end={project.end}
            variant="borderless"
            size="small"
            style={{ width: '100%', marginTop: 'auto' }}
            dateFormat={timelineFormat}
          />
        )}
      </Flex>
    </Card>
  )
}

export interface ProjectsCardViewProps {
  projects: ProjectListDto[] | undefined
  isLoading: boolean
  viewSelector?: ReactNode
  onCardClick: (key: string) => void
  hidePortfolio?: boolean
}

const ProjectsCardView: FC<ProjectsCardViewProps> = ({
  projects,
  isLoading,
  viewSelector,
  onCardClick,
  hidePortfolio,
}) => {
  if (isLoading) {
    return (
      <Flex vertical gap="small">
        {viewSelector && <Flex justify="flex-end">{viewSelector}</Flex>}
        <Flex justify="center" style={{ padding: 24 }}>
          <Spin />
        </Flex>
      </Flex>
    )
  }

  const sortedProjects = [...(projects ?? [])].sort((a, b) =>
    a.name.localeCompare(b.name, undefined, {
      numeric: true,
      sensitivity: 'base',
    }),
  )

  return (
    <Flex vertical gap="small">
      {viewSelector && <Flex justify="flex-end">{viewSelector}</Flex>}
      <div className={styles.grid}>
        {sortedProjects.map((project) => (
          <ProjectCard
            key={project.key}
            project={project}
            onCardClick={onCardClick}
            hidePortfolio={hidePortfolio}
          />
        ))}
      </div>
    </Flex>
  )
}

export default ProjectsCardView
