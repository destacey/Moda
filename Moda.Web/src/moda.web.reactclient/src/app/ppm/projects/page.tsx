'use client'

import { PageTitle } from '@/src/components/common'
import useAuth from '@/src/components/contexts/auth'
import { authorizePage } from '@/src/components/hoc'
import { useDocumentTitle } from '@/src/hooks'
import { useGetProjectsQuery } from '@/src/store/features/ppm/projects-api'
import { Button } from 'antd'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { CreateProjectForm } from './_components'
import { ProjectsFilterBar, ProjectsGrid } from '../_components'
import { useMessage } from '@/src/components/contexts/messaging'

// Project status enum values matching the backend
const PROJECT_STATUS = {
  Proposed: 1,
  Active: 2,
  Completed: 3,
  Cancelled: 4,
} as const

const DEFAULT_STATUSES = [
  PROJECT_STATUS.Proposed,
  PROJECT_STATUS.Active,
]

const ProjectsPage: React.FC = () => {
  useDocumentTitle('Projects')
  const [openCreateProjectForm, setOpenCreateProjectForm] =
    useState<boolean>(false)
  const [selectedStatuses, setSelectedStatuses] =
    useState<number[]>(DEFAULT_STATUSES)
  const [selectedPortfolioId, setSelectedPortfolioId] = useState<
    string | undefined
  >(undefined)
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
  })

  useEffect(() => {
    if (error) {
      console.error(error)
      messageApi.error('Failed to load projects.')
    }
  }, [error, messageApi])

  const toggleStatus = useCallback((statusId: number) => {
    setSelectedStatuses((prev) => {
      if (prev.includes(statusId)) {
        return prev.filter((s) => s !== statusId)
      }
      return [...prev, statusId]
    })
  }, [])

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
        onToggleStatus={toggleStatus}
        selectedPortfolioId={selectedPortfolioId}
        onPortfolioChange={setSelectedPortfolioId}
      />
      <ProjectsGrid
        projects={projectData}
        isLoading={isLoading}
        refetch={refetch}
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
