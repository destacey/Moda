'use client'

import { ModaEmpty } from '@/src/components/common'
import useAuth from '@/src/components/contexts/auth'
import { ProjectListDto } from '@/src/services/moda-api'
import { useGetProjectsPlanSummariesQuery } from '@/src/store/features/ppm/projects-api'
import { Flex, Spin } from 'antd'
import { FC, useMemo } from 'react'
import { PortfolioGroup, sortProjects } from './project-card-helpers'
import PortfolioGroupSection from './portfolio-group-section'
import styles from '../my-projects-dashboard.module.css'

export interface PortfolioGroupListProps {
  projects: ProjectListDto[] | undefined
  isLoading: boolean
  selectedProjectKey: string | null
  selectedRoles: number[]
  onSelectProject: (key: string) => void
}

const PortfolioGroupList: FC<PortfolioGroupListProps> = ({
  projects,
  isLoading,
  selectedProjectKey,
  selectedRoles,
  onSelectProject,
}) => {
  const { user } = useAuth()

  const projectIds = useMemo(
    () => projects?.map((p) => p.id) ?? [],
    [projects],
  )

  const { data: planSummaries } = useGetProjectsPlanSummariesQuery(
    {
      projectIds,
      role: selectedRoles.length > 0 ? selectedRoles : undefined,
    },
    { skip: projectIds.length === 0 },
  )

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
    return <ModaEmpty message="No projects found" />
  }

  return (
    <Flex vertical gap={4}>
      {groups.map((group) => (
        <PortfolioGroupSection
          key={group.portfolioId}
          group={group}
          selectedProjectKey={selectedProjectKey}
          employeeId={user.employeeId}
          planSummaries={planSummaries}
          onSelectProject={onSelectProject}
        />
      ))}
    </Flex>
  )
}

export default PortfolioGroupList
