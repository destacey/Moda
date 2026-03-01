'use client'

import PageTitle from '@/src/components/common/page-title'
import { useCallback, useState } from 'react'
import { useDocumentTitle } from '@/src/hooks'
import { CreatePokerSessionForm, PokerSessionsGrid } from './_components'
import useAuth from '@/src/components/contexts/auth'
import { Button } from 'antd'
import { authorizePage } from '@/src/components/hoc'
import { useGetPokerSessionsQuery } from '@/src/store/features/planning/poker-sessions-api'

const PokerSessionsPage = () => {
  useDocumentTitle('Planning Poker')

  const [openCreateForm, setOpenCreateForm] = useState(false)

  const {
    data: sessionsData,
    isLoading,
    refetch,
  } = useGetPokerSessionsQuery(undefined)

  const { hasPermissionClaim } = useAuth()
  const canCreate = hasPermissionClaim('Permissions.PokerSessions.Create')

  const refresh = useCallback(() => {
    refetch()
  }, [refetch])

  const onCreateFormClosed = (wasCreated: boolean) => {
    setOpenCreateForm(false)
    if (wasCreated) {
      refetch()
    }
  }

  const actions = () => (
    <>
      {canCreate && (
        <Button onClick={() => setOpenCreateForm(true)}>
          Create Session
        </Button>
      )}
    </>
  )

  return (
    <>
      <br />
      <PageTitle title="Planning Poker" actions={canCreate && actions()} />
      <PokerSessionsGrid
        sessions={sessionsData}
        isLoading={isLoading}
        refetch={refresh}
      />
      {openCreateForm && (
        <CreatePokerSessionForm
          onFormCreate={() => onCreateFormClosed(true)}
          onFormCancel={() => onCreateFormClosed(false)}
        />
      )}
    </>
  )
}

const PokerSessionsPageWithAuthorization = authorizePage(
  PokerSessionsPage,
  'Permission',
  'Permissions.PokerSessions.View',
)

export default PokerSessionsPageWithAuthorization
