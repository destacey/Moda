'use client'

import PageTitle from '@/src/components/common/page-title'
import { useDocumentTitle } from '@/src/hooks'
import { usePokerSessionConnection } from '@/src/hooks/use-poker-session-connection'
import { authorizePage } from '@/src/components/hoc'
import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import {
  useGetPokerSessionQuery,
  useActivatePokerSessionMutation,
  useCompletePokerSessionMutation,
  useRemovePokerRoundMutation,
} from '@/src/store/features/planning/poker-sessions-api'
import { Button, Col, Empty, Row, Space, Spin, Tag } from 'antd'
import { useParams } from 'next/navigation'
import { FC, useCallback, useState } from 'react'
import {
  AddRoundForm,
  CurrentRoundView,
  RoundList,
} from './_components'

const PokerSessionDetailPage: FC = () => {
  const { key } = useParams<{ key: string }>()
  useDocumentTitle(`Poker Session ${key}`)

  const messageApi = useMessage()
  const { hasPermissionClaim } = useAuth()
  const canManage = hasPermissionClaim('Permissions.PokerSessions.Update')

  const { data: session, isLoading } = useGetPokerSessionQuery(key, {
    pollingInterval: 5000,
  })

  // Real-time updates via SignalR (falls back to polling above)
  usePokerSessionConnection(session?.id, session?.key)

  const [activateSession, { isLoading: isActivating }] =
    useActivatePokerSessionMutation()
  const [completeSession, { isLoading: isCompleting }] =
    useCompletePokerSessionMutation()
  const [removeRound] = useRemovePokerRoundMutation()

  const [openAddRoundForm, setOpenAddRoundForm] = useState(false)
  const [selectedRoundId, setSelectedRoundId] = useState<string | undefined>()

  const handleActivate = useCallback(async () => {
    if (!session) return
    try {
      const response = await activateSession({
        id: session.id,
        key: session.key,
      })
      if (response.error) throw response.error
      messageApi.success('Session activated.')
    } catch {
      messageApi.error('Failed to activate session.')
    }
  }, [activateSession, session, messageApi])

  const handleComplete = useCallback(async () => {
    if (!session) return
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
  }, [completeSession, session, messageApi])

  const handleRemoveRound = useCallback(
    async (roundId: string) => {
      if (!session) return
      try {
        const response = await removeRound({
          sessionId: session.id,
          roundId,
          sessionKey: session.key,
        })
        if (response.error) throw response.error
        if (selectedRoundId === roundId) {
          setSelectedRoundId(undefined)
        }
      } catch {
        messageApi.error('Failed to remove round.')
      }
    },
    [removeRound, session, selectedRoundId, messageApi],
  )

  if (isLoading) {
    return <Spin size="large" />
  }

  if (!session) {
    return <Empty description="Poker session not found." />
  }

  const isCreated = session.status === 'Created'
  const isActive = session.status === 'Active'
  const isCompleted = session.status === 'Completed'

  const rounds = session.rounds ?? []
  const activeRound = selectedRoundId
    ? rounds.find((r) => r.id === selectedRoundId)
    : rounds.find(
        (r) => r.status === 'Voting' || r.status === 'Revealed',
      ) ?? rounds[0]

  const statusColor = isCreated
    ? 'default'
    : isActive
      ? 'processing'
      : 'success'

  const actions = () => (
    <Space>
      {canManage && isCreated && (
        <Button type="primary" onClick={handleActivate} loading={isActivating}>
          Activate Session
        </Button>
      )}
      {canManage && isActive && (
        <>
          <Button onClick={() => setOpenAddRoundForm(true)}>Add Round</Button>
          <Button danger onClick={handleComplete} loading={isCompleting}>
            Complete Session
          </Button>
        </>
      )}
    </Space>
  )

  return (
    <>
      <PageTitle
        title={
          <>
            {session.name}{' '}
            <Tag color={statusColor}>{session.status}</Tag>
          </>
        }
        subtitle={`Facilitator: ${session.facilitatorName}`}
        actions={canManage && actions()}
      />

      <Row gutter={16}>
        <Col xs={24} md={7} lg={6}>
          <RoundList
            rounds={rounds}
            activeRoundId={activeRound?.id}
            onSelectRound={setSelectedRoundId}
            onRemoveRound={handleRemoveRound}
            canManage={canManage && isActive}
          />
        </Col>
        <Col xs={24} md={17} lg={18}>
          {activeRound ? (
            <CurrentRoundView
              round={activeRound}
              sessionId={session.id}
              sessionKey={session.key}
              estimationScale={session.estimationScale}
              canManage={canManage && isActive}
            />
          ) : (
            <Empty description={isActive ? 'Add a round to get started.' : 'No rounds.'} />
          )}
        </Col>
      </Row>

      {openAddRoundForm && session && (
        <AddRoundForm
          sessionId={session.id}
          sessionKey={session.key}
          onFormCreate={() => setOpenAddRoundForm(false)}
          onFormCancel={() => setOpenAddRoundForm(false)}
        />
      )}
    </>
  )
}

const PokerSessionDetailPageWithAuthorization = authorizePage(
  PokerSessionDetailPage,
  'Permission',
  'Permissions.PokerSessions.View',
)

export default PokerSessionDetailPageWithAuthorization
