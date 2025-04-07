'use client'

import { PageTitle } from '@/src/components/common'
import useAuth from '@/src/components/contexts/auth'
import { authorizePage } from '@/src/components/hoc'
import { useDocumentTitle } from '@/src/hooks'
import { useGetStrategicInitiativesQuery } from '@/src/store/features/ppm/strategic-initiatives-api'
import { Button } from 'antd'
import { useEffect, useMemo, useState } from 'react'
import { CreateStrategicInitiativeForm } from './_components'
import { StrategicInitiativesGrid } from '../_components'
import { useMessage } from '@/src/components/contexts/messaging'

const StrategicInitiativesPage: React.FC = () => {
  useDocumentTitle('Strategic Initiatives')
  const [
    openCreateStrategicInitiativeForm,
    setOpenCreateStrategicInitiativeForm,
  ] = useState<boolean>(false)

  const messageApi = useMessage()

  const { hasPermissionClaim } = useAuth()
  const canCreateStrategicInitiative = hasPermissionClaim(
    'Permissions.StrategicInitiatives.Create',
  )
  const showActions = canCreateStrategicInitiative

  const {
    data: strategicInitiativeData,
    isLoading,
    error,
    refetch,
  } = useGetStrategicInitiativesQuery(null)

  useEffect(() => {
    if (error) {
      console.error(error)
      messageApi.error('Failed to load strategic initiatives.')
    }
  }, [error, messageApi])

  const actions = useMemo(() => {
    if (!showActions) return null
    return (
      <>
        {canCreateStrategicInitiative && (
          <Button onClick={() => setOpenCreateStrategicInitiativeForm(true)}>
            Create Strategic Initiative
          </Button>
        )}
      </>
    )
  }, [canCreateStrategicInitiative, showActions])

  const onCreateStrategicInitiativeFormClosed = (wasCreated: boolean) => {
    setOpenCreateStrategicInitiativeForm(false)
    if (wasCreated) {
      refetch()
    }
  }

  return (
    <>
      <PageTitle title="Strategic Initiatives" actions={actions} />
      <StrategicInitiativesGrid
        strategicInitiatives={strategicInitiativeData}
        isLoading={isLoading}
        refetch={refetch}
      />
      {openCreateStrategicInitiativeForm && (
        <CreateStrategicInitiativeForm
          showForm={openCreateStrategicInitiativeForm}
          onFormComplete={() => onCreateStrategicInitiativeFormClosed(true)}
          onFormCancel={() => onCreateStrategicInitiativeFormClosed(false)}
        />
      )}
    </>
  )
}

const StrategicInitiativesPageWithAuthorization = authorizePage(
  StrategicInitiativesPage,
  'Permission',
  'Permissions.StrategicInitiatives.View',
)

export default StrategicInitiativesPageWithAuthorization
