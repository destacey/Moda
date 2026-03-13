'use client'

import PageTitle from '@/src/components/common/page-title'
import { useCallback, useMemo, useState } from 'react'
import { useDocumentTitle } from '@/src/hooks'
import {
  CreatePokerSessionForm,
  DeletePokerSessionForm,
  PokerSessionsGrid,
} from './_components'
import EditPokerSessionForm from './[key]/_components/edit-poker-session-form'
import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { Button } from 'antd'
import { authorizePage, requireFeatureFlag } from '@/src/components/hoc'
import { ControlItemSwitch } from '@/src/components/common/control-items-menu'
import { ItemType } from 'antd/es/menu/interface'
import { PokerSessionListDto, PokerSessionStatus } from '@/src/services/moda-api'
import {
  useGetPokerSessionsQuery,
  useCompletePokerSessionMutation,
} from '@/src/store/features/planning/poker-sessions-api'

const PokerSessionsPage = () => {
  useDocumentTitle('Planning Poker')

  const [openCreateForm, setOpenCreateForm] = useState(false)
  const [includeCompleted, setIncludeCompleted] = useState(false)
  const [editSessionKey, setEditSessionKey] = useState<number | null>(null)
  const [deleteSession, setDeleteSession] =
    useState<PokerSessionListDto | null>(null)

  const messageApi = useMessage()

  const {
    data: sessionsData,
    isLoading,
    refetch,
  } = useGetPokerSessionsQuery(
    includeCompleted ? undefined : PokerSessionStatus.Active,
  )

  const [completeSession] = useCompletePokerSessionMutation()

  const { hasPermissionClaim } = useAuth()
  const canCreate = hasPermissionClaim('Permissions.PokerSessions.Create')
  const canUpdate = hasPermissionClaim('Permissions.PokerSessions.Update')
  const canDelete = hasPermissionClaim('Permissions.PokerSessions.Delete')

  const controlItems = useMemo<ItemType[]>(
    () => [
      {
        label: (
          <ControlItemSwitch
            label="Include Completed"
            checked={includeCompleted}
            onChange={setIncludeCompleted}
          />
        ),
        key: 'include-completed',
        onClick: () => setIncludeCompleted((prev) => !prev),
      },
    ],
    [includeCompleted],
  )

  const refresh = useCallback(() => {
    refetch()
  }, [refetch])

  const onCreateFormClosed = (wasCreated: boolean) => {
    setOpenCreateForm(false)
    if (wasCreated) {
      refetch()
    }
  }

  const handleEdit = useCallback((session: PokerSessionListDto) => {
    setEditSessionKey(session.key)
  }, [])

  const handleComplete = useCallback(
    async (session: PokerSessionListDto) => {
      try {
        const response = await completeSession({
          id: session.id,
          key: session.key,
        })
        if (response.error) throw response.error
        messageApi.success('Session completed.')
      } catch {
        messageApi.error('Failed to complete session.')
      }
    },
    [completeSession, messageApi],
  )

  const handleDelete = useCallback((session: PokerSessionListDto) => {
    setDeleteSession(session)
  }, [])

  const onEditFormClosed = useCallback(() => {
    setEditSessionKey(null)
  }, [])

  const onDeleteFormClosed = useCallback(
    (wasDeleted: boolean) => {
      setDeleteSession(null)
      if (wasDeleted) {
        refetch()
      }
    },
    [refetch],
  )

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
        canUpdate={canUpdate}
        canDelete={canDelete}
        gridControlMenuItems={controlItems}
        onEditClicked={handleEdit}
        onCompleteClicked={handleComplete}
        onDeleteClicked={handleDelete}
      />
      {openCreateForm && (
        <CreatePokerSessionForm
          onFormCreate={() => onCreateFormClosed(true)}
          onFormCancel={() => onCreateFormClosed(false)}
        />
      )}
      {editSessionKey && (
        <EditPokerSessionForm
          sessionKey={editSessionKey}
          onFormUpdate={() => onEditFormClosed()}
          onFormCancel={() => onEditFormClosed()}
        />
      )}
      {deleteSession && (
        <DeletePokerSessionForm
          session={deleteSession}
          onFormComplete={() => onDeleteFormClosed(true)}
          onFormCancel={() => onDeleteFormClosed(false)}
        />
      )}
    </>
  )
}

const PokerSessionsPageWithAuthorization = requireFeatureFlag(
  authorizePage(PokerSessionsPage, 'Permission', 'Permissions.PokerSessions.View'),
  'planning-poker',
)

export default PokerSessionsPageWithAuthorization
