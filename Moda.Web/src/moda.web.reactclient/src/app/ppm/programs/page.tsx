'use client'

import { PageTitle } from '@/src/components/common'
import useAuth from '@/src/components/contexts/auth'
import { authorizePage } from '@/src/components/hoc'
import { useDocumentTitle } from '@/src/hooks'
import { useGetProgramsQuery } from '@/src/store/features/ppm/programs-api'
import { Button } from 'antd'
import { FC, useEffect, useMemo, useState } from 'react'
import { CreateProgramForm } from './_components'
import { ProgramsGrid } from '../_components'
import { useMessage } from '@/src/components/contexts/messaging'

const ProgramsPage: FC = () => {
  useDocumentTitle('Programs')
  const [openCreateProgramForm, setOpenCreateProgramForm] =
    useState<boolean>(false)
  const messageApi = useMessage()

  const { hasPermissionClaim } = useAuth()
  const canCreateProgram = hasPermissionClaim('Permissions.Programs.Create')
  const showActions = canCreateProgram

  const {
    data: programData,
    isLoading,
    error,
    refetch,
  } = useGetProgramsQuery(null)

  useEffect(() => {
    if (error) {
      console.error(error)
      messageApi.error('Failed to load programs.')
    }
  }, [error, messageApi])

  const actions = useMemo(() => {
    if (!showActions) return null
    return (
      <>
        {canCreateProgram && (
          <Button onClick={() => setOpenCreateProgramForm(true)}>
            Create Program
          </Button>
        )}
      </>
    )
  }, [canCreateProgram, showActions])

  const onCreateProgramFormClosed = (wasCreated: boolean) => {
    setOpenCreateProgramForm(false)
    if (wasCreated) {
      refetch()
    }
  }

  return (
    <>
      <PageTitle title="Programs" actions={actions} />
      <ProgramsGrid
        programs={programData}
        isLoading={isLoading}
        refetch={refetch}
      />
      {openCreateProgramForm && (
        <CreateProgramForm
          showForm={openCreateProgramForm}
          onFormComplete={() => onCreateProgramFormClosed(true)}
          onFormCancel={() => onCreateProgramFormClosed(false)}
        />
      )}
    </>
  )
}

const ProgramsPageWithAuthorization = authorizePage(
  ProgramsPage,
  'Permission',
  'Permissions.Programs.View',
)

export default ProgramsPageWithAuthorization
