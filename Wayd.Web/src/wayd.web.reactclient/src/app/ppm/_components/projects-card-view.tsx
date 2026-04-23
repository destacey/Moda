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

const { Text } = Typography

interface ProjectCardProps {
  project: ProjectListDto
  onCardClick: (key: string) => void
}

const ProjectCard: FC<ProjectCardProps> = ({ project, onCardClick }) => {
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
      <Flex vertical gap={8} style={{ flex: 1 }}>
        {/* Header */}
        <Flex justify="space-between" align="flex-start" gap={4}>
          <Flex vertical gap={2} style={{ flex: 1, minWidth: 0 }}>
            <Text type="secondary" style={{ fontSize: 11 }}>
              {project.key}
            </Text>
            <Link
              href={`/ppm/projects/${project.key}`}
              onClick={(e) => e.stopPropagation()}
              style={{ width: 'fit-content' }}
            >
              {project.name}
            </Link>
          </Flex>
          <LifecycleStatusTag status={project.status} />
        </Flex>

        {/* Meta rows */}
        <Flex vertical gap={3}>
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
}

const ProjectsCardView: FC<ProjectsCardViewProps> = ({
  projects,
  isLoading,
  viewSelector,
  onCardClick,
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
          />
        ))}
      </div>
    </Flex>
  )
}

export default ProjectsCardView
