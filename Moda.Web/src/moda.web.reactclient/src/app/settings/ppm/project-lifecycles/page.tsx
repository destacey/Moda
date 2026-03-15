'use client'

import { ModaGrid, PageTitle } from '@/src/components/common'
import useAuth from '@/src/components/contexts/auth'
import { authorizePage } from '@/src/components/hoc'
import { useDocumentTitle } from '@/src/hooks'
import { ProjectLifecycleListDto } from '@/src/services/moda-api'
import { useGetProjectLifecyclesQuery } from '@/src/store/features/ppm/project-lifecycles-api'
import { ColDef } from 'ag-grid-community'
import { Button } from 'antd'
import Link from 'next/link'
import { useCallback, useEffect, useMemo, useState } from 'react'
import CreateProjectLifecycleForm from './_components/create-project-lifecycle-form'
import { useMessage } from '@/src/components/contexts/messaging'

const ProjectLifecycleCellRenderer = ({ value, data }) => {
  return <Link href={`./project-lifecycles/${data.key}`}>{value}</Link>
}

const ProjectLifecyclesPage = () => {
  useDocumentTitle('PPM - Project Lifecycles')
  const [openCreateForm, setOpenCreateForm] = useState<boolean>(false)

  const messageApi = useMessage()

  const {
    data: lifecycleData,
    isLoading,
    error,
    refetch,
  } = useGetProjectLifecyclesQuery(null)

  const { hasPermissionClaim } = useAuth()
  const canCreateProjectLifecycle = hasPermissionClaim(
    'Permissions.ProjectLifecycles.Create',
  )
  const showActions = canCreateProjectLifecycle

  useEffect(() => {
    if (error) {
      messageApi.error(
        error.detail ??
          'An error occurred while loading project lifecycles',
      )
      console.error(error)
    }
  }, [error, messageApi])

  const columnDefs = useMemo<ColDef<ProjectLifecycleListDto>[]>(
    () => [
      { field: 'id', hide: true },
      { field: 'key', width: 90 },
      { field: 'name', cellRenderer: ProjectLifecycleCellRenderer, sort: 'asc' },
      { field: 'state.name', headerName: 'State', width: 100 },
      { field: 'phaseCount', headerName: 'Phase Count', width: 120 },
    ],
    [],
  )

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

  const actions = useMemo(() => {
    if (!showActions) return null
    return (
      <>
        {canCreateProjectLifecycle && (
          <Button onClick={() => setOpenCreateForm(true)}>
            Create Project Lifecycle
          </Button>
        )}
      </>
    )
  }, [canCreateProjectLifecycle, showActions])

  const onCreateFormClosed = (wasCreated: boolean) => {
    setOpenCreateForm(false)
    if (wasCreated) {
      refetch()
    }
  }

  return (
    <>
      <PageTitle title="Project Lifecycles" actions={actions} />

      <ModaGrid
        height={600}
        columnDefs={columnDefs}
        rowData={lifecycleData}
        loadData={refresh}
        loading={isLoading}
      />
      {openCreateForm && (
        <CreateProjectLifecycleForm
          onFormComplete={() => onCreateFormClosed(true)}
          onFormCancel={() => onCreateFormClosed(false)}
        />
      )}
    </>
  )
}

const ProjectLifecyclesPageWithAuthorization = authorizePage(
  ProjectLifecyclesPage,
  'Permission',
  'Permissions.ProjectLifecycles.View',
)

export default ProjectLifecyclesPageWithAuthorization
