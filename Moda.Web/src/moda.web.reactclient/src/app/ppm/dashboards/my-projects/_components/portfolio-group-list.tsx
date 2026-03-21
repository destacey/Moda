'use client'

import useAuth from '@/src/components/contexts/auth'
import { ProjectListDto } from '@/src/services/moda-api'
import { Flex, Spin, Typography } from 'antd'
import { FC, useMemo } from 'react'
import { PortfolioGroup, sortProjects } from './project-card-helpers'
import PortfolioGroupSection from './portfolio-group-section'
import styles from '../my-projects-dashboard.module.css'

const { Text } = Typography

export interface PortfolioGroupListProps {
  projects: ProjectListDto[] | undefined
  isLoading: boolean
  selectedProjectKey: string | null
  onSelectProject: (key: string) => void
}

const PortfolioGroupList: FC<PortfolioGroupListProps> = ({
  projects,
  isLoading,
  selectedProjectKey,
  onSelectProject,
}) => {
  const { user } = useAuth()

  const groups = useMemo<PortfolioGroup[]>(() => {
    if (!projects) return []

    const map = new Map<string, PortfolioGroup>()
    for (const project of projects) {
      const portfolioId = project.portfolio?.id ?? 'unassigned'
      const portfolioName = project.portfolio?.name ?? 'Unassigned'
      if (!map.has(portfolioId)) {
        map.set(portfolioId, { portfolioId, portfolioName, projects: [] })
      }
      map.get(portfolioId)!.projects.push(project)
    }

    return Array.from(map.values())
      .map((group) => ({
        ...group,
        projects: sortProjects(group.projects),
      }))
      .sort((a, b) => a.portfolioName.localeCompare(b.portfolioName))
  }, [projects])

  if (isLoading) {
    return (
      <Flex justify="center" className={styles.emptyState}>
        <Spin />
      </Flex>
    )
  }

  if (groups.length === 0) {
    return (
      <Flex justify="center" className={styles.emptyState}>
        <Text type="secondary">No projects found</Text>
      </Flex>
    )
  }

  return (
    <Flex vertical gap={4}>
      {groups.map((group) => (
        <PortfolioGroupSection
          key={group.portfolioId}
          group={group}
          selectedProjectKey={selectedProjectKey}
          employeeId={user.employeeId}
          onSelectProject={onSelectProject}
        />
      ))}
    </Flex>
  )
}

export default PortfolioGroupList
