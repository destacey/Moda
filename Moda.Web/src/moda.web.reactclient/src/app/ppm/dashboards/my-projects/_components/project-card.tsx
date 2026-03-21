'use client'

import { LifecycleStatusTag } from '@/src/components/common'
import PhaseTimeline from '@/src/app/ppm/_components/phase-timeline'
import { ProjectListDto } from '@/src/services/moda-api'
import { Card, Flex, Typography } from 'antd'
import dayjs from 'dayjs'
import { FC, useMemo } from 'react'
import { collectTeamMembers, getUserRoles } from './project-card-helpers'
import ProjectStatPills from './project-stat-pills'
import TeamAvatars from './team-avatars'
import styles from '../my-projects-dashboard.module.css'

const { Text } = Typography

export interface ProjectCardProps {
  project: ProjectListDto
  isSelected: boolean
  employeeId: string | null
  onSelect: (key: string) => void
}

const ProjectCard: FC<ProjectCardProps> = ({
  project,
  isSelected,
  employeeId,
  onSelect,
}) => {
  const roles = useMemo(
    () => getUserRoles(project, employeeId),
    [project, employeeId],
  )
  const teamMembers = useMemo(() => collectTeamMembers(project), [project])
  const endDate = project.end
    ? dayjs(project.end).format('MMM D, YYYY')
    : null

  return (
    <Card
      size="small"
      hoverable
      className={isSelected ? styles.projectCardSelected : undefined}
      onClick={() => onSelect(project.key)}
    >
      <Flex vertical gap={10}>
        {/* Header row: role + status */}
        <Flex vertical gap={4}>
          <Flex
            align="center"
            justify={roles.length > 0 ? 'space-between' : 'flex-end'}
          >
            {roles.length > 0 && (
              <Text type="secondary" className={styles.roleBadge}>
                {roles.join(' · ')}
              </Text>
            )}
            <LifecycleStatusTag status={project.status} />
          </Flex>
          <Text type="secondary" style={{ fontSize: 11 }}>
            {project.key}
          </Text>
          <Text className={styles.projectName} ellipsis>
            {project.name}
          </Text>
        </Flex>

        {/* Phase timeline */}
        {project.phases?.length > 0 && (
          <PhaseTimeline phases={project.phases} size="small" />
        )}

        {/* Stat pills */}
        <ProjectStatPills projectKey={project.key} />

        {/* Footer: avatars + end date */}
        <div className={styles.cardFooter}>
          <TeamAvatars members={teamMembers} />
          {endDate && <span className={styles.endDate}>End {endDate}</span>}
        </div>
      </Flex>
    </Card>
  )
}

export default ProjectCard
