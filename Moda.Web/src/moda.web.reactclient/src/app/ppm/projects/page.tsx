'use client'

import { PageTitle } from '@/src/components/common'
import useAuth from '@/src/components/contexts/auth'
import { authorizePage } from '@/src/components/hoc'
import { useDocumentTitle, useLocalStorageState } from '@/src/hooks'
import { useGetProjectsQuery } from '@/src/store/features/ppm/projects-api'
import { Button } from 'antd'
import { FC, useCallback, useEffect, useMemo, useState } from 'react'
import { CreateProjectForm } from './_components'
import { ProjectsFilterBar, ProjectsGrid } from '../_components'
import { useMessage } from '@/src/components/contexts/messaging'

// Project status enum values matching the backend
const PROJECT_STATUS = {
  Proposed: 1,
  Approved: 5,
  Active: 2,
  Completed: 3,
  Cancelled: 4,
} as const

const DEFAULT_STATUSES = [PROJECT_STATUS.Approved, PROJECT_STATUS.Active]

const ALL_ROLES = [1, 2, 3, 4, 5]

const getRoleFilterValues = (
  selectedRole: string | null,
): number[] | undefined => {
  if (!selectedRole) return undefined
  if (selectedRole === 'all') return ALL_ROLES
  return [parseInt(selectedRole)]
}

const ProjectsPage: FC = () => {
  useDocumentTitle('Projects')
  const [openCreateProjectForm, setOpenCreateProjectForm] =
    useState<boolean>(false)
  const [selectedStatuses, setSelectedStatuses] =
    useLocalStorageState<number[]>('projects-filter-statuses', DEFAULT_STATUSES)
  const [selectedPortfolioId, setSelectedPortfolioId] = useLocalStorageState<
    string | null
  >('projects-filter-portfolio', null)
  const [selectedRole, setSelectedRole] = useLocalStorageState<
    string | null
  >('projects-filter-role', null)
  const messageApi = useMessage()

  const { hasPermissionClaim } = useAuth()
  const canCreateProject = hasPermissionClaim('Permissions.Projects.Create')
  const showActions = canCreateProject

  const {
    data: projectData,
    isLoading,
    error,
    refetch,
  } = useGetProjectsQuery({
    status: selectedStatuses.length > 0 ? selectedStatuses : undefined,
    portfolioId: selectedPortfolioId,
    role: getRoleFilterValues(selectedRole),
  })

  useEffect(() => {
    if (error) {
      console.error(error)
      messageApi.error('Failed to load projects.')
    }
  }, [error, messageApi])

  const handleResetFilters = useCallback(() => {
    setSelectedStatuses(DEFAULT_STATUSES)
    setSelectedPortfolioId(null)
    setSelectedRole(null)
  }, [setSelectedStatuses, setSelectedPortfolioId, setSelectedRole])

  const actions = useMemo(() => {
    if (!showActions) return null
    return (
      <>
        {canCreateProject && (
          <Button onClick={() => setOpenCreateProjectForm(true)}>
            Create Project
          </Button>
        )}
      </>
    )
  }, [canCreateProject, showActions])

  const onCreateProjectFormClosed = (wasCreated: boolean) => {
    setOpenCreateProjectForm(false)
    if (wasCreated) {
      refetch()
    }
  }

  return (
    <>
      <PageTitle title="Projects" actions={actions} />
      <ProjectsFilterBar
        selectedStatuses={selectedStatuses}
        onStatusChange={setSelectedStatuses}
        selectedPortfolioId={selectedPortfolioId}
        onPortfolioChange={setSelectedPortfolioId}
        selectedRole={selectedRole}
        onRoleChange={setSelectedRole}
        onReset={handleResetFilters}
      />
      <ProjectsGrid
        projects={projectData}
        isLoading={isLoading}
        refetch={refetch}
        gridHeight={600}
      />
      {openCreateProjectForm && (
        <CreateProjectForm
          onFormComplete={() => onCreateProjectFormClosed(true)}
          onFormCancel={() => onCreateProjectFormClosed(false)}
        />
      )}
    </>
  )
}

const ProjectsPageWithAuthorization = authorizePage(
  ProjectsPage,
  'Permission',
  'Permissions.Projects.View',
)

export default ProjectsPageWithAuthorization
