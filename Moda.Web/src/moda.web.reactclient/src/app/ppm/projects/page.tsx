'use client'

import { PageTitle } from '@/src/components/common'
import useAuth from '@/src/components/contexts/auth'
import { authorizePage } from '@/src/components/hoc'
import { useDocumentTitle } from '@/src/hooks'
import { useGetProjectsQuery } from '@/src/store/features/ppm/projects-api'
import { Button } from 'antd'
import { useEffect, useMemo, useState } from 'react'
import { CreateProjectForm } from './_components'
import { ProjectsGrid } from '../_components'
import { useMessage } from '@/src/components/contexts/messaging'

const ProjectsPage: React.FC = () => {
  useDocumentTitle('Projects')
  const [openCreateProjectForm, setOpenCreateProjectForm] =
    useState<boolean>(false)
  const messageApi = useMessage();

  const { hasPermissionClaim } = useAuth()
  const canCreateProject = hasPermissionClaim('Permissions.Projects.Create')
  const showActions = canCreateProject

  const {
    data: projectData,
    isLoading,
    error,
    refetch,
  } = useGetProjectsQuery(null)

  useEffect(() => {
    if (error) {
      console.error(error)
      messageApi.error('Failed to load projects.')
    }
  }, [error, messageApi])

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
      <ProjectsGrid
        projects={projectData}
        isLoading={isLoading}
        refetch={refetch}
        messageApi={messageApi}
      />
      {openCreateProjectForm && (
        <CreateProjectForm
          showForm={openCreateProjectForm}
          onFormComplete={() => onCreateProjectFormClosed(true)}
          onFormCancel={() => onCreateProjectFormClosed(false)}
          messageApi={messageApi}
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
