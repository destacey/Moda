'use client'

import { useDocumentTitle } from '@/src/hooks'
import { usePokerSessionConnection } from '@/src/hooks/use-poker-session-connection'
import { authorizePage } from '@/src/components/hoc'
import useAuth from '@/src/components/contexts/auth'
import useTheme from '@/src/components/contexts/theme'
import { useMessage } from '@/src/components/contexts/messaging'
import {
  useGetPokerSessionQuery,
  useCompletePokerSessionMutation,
  useRemovePokerRoundMutation,
} from '@/src/store/features/planning/poker-sessions-api'
import { Avatar, Button, Col, Empty, Flex, Row, Spin, Tag, Tooltip } from 'antd'
import { useParams } from 'next/navigation'
import { LinkOutlined } from '@ant-design/icons'
import { CSSProperties, FC, useCallback, useMemo, useState } from 'react'
import { PresenceParticipant } from '@/src/hooks/use-poker-session-connection'
import { SessionHeader, SessionSidebar, VotingArea } from './_components'

interface PokerCssVars extends CSSProperties {
  '--poker-bg': string
  '--poker-border': string
  '--poker-text': string
  '--poker-text-secondary': string
  '--poker-muted': string
  '--poker-primary': string
  '--poker-primary-bg': string
  '--poker-success': string
  '--poker-active-bg': string
  '--poker-hover-bg': string
}

const PokerSessionDetailPage: FC = () => {
  const { key } = useParams<{ key: string }>()
  useDocumentTitle(`Poker Session ${key}`)

  const { token } = useTheme()
  const messageApi = useMessage()
  const { hasPermissionClaim } = useAuth()
  const canManage = hasPermissionClaim('Permissions.PokerSessions.Update')

  const { data: session, isLoading } = useGetPokerSessionQuery(key, {
    pollingInterval: 5000,
  })

  const [connectedParticipants, setConnectedParticipants] = useState<
    PresenceParticipant[]
  >([])

  // Real-time updates via SignalR (falls back to polling above)
  usePokerSessionConnection(session?.id, session?.key, setConnectedParticipants)

  const [completeSession, { isLoading: isCompleting }] =
    useCompletePokerSessionMutation()
  const [removeRound] = useRemovePokerRoundMutation()

  const [selectedRoundId, setSelectedRoundId] = useState<string | undefined>()

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

  const handleCopyInviteLink = useCallback(() => {
    navigator.clipboard.writeText(window.location.href)
    messageApi.success('Invite link copied to clipboard.')
  }, [messageApi])

  const rounds = session?.rounds ?? []
  const activeRound = selectedRoundId
    ? rounds.find((r) => r.id === selectedRoundId)
    : rounds.find(
        (r) => r.status === 'Voting' || r.status === 'Revealed',
      ) ?? rounds[0]

  // Merge connected participants with voters from the active round
  // so that votes from disconnected users still appear
  const roundParticipants = useMemo(() => {
    const map = new Map<string, { id: string; name: string }>()
    for (const p of connectedParticipants) {
      map.set(p.id, p)
    }
    if (activeRound) {
      for (const vote of activeRound.votes ?? []) {
        if (vote.participant && !map.has(vote.participant.id)) {
          map.set(vote.participant.id, {
            id: vote.participant.id,
            name: vote.participant.name,
          })
        }
      }
    }
    return Array.from(map.values())
  }, [connectedParticipants, activeRound])

  if (isLoading) {
    return <Spin size="large" />
  }

  if (!session) {
    return <Empty description="Poker session not found." />
  }

  const isActive = session.status === 'Active'

  const cssVars: PokerCssVars = {
    '--poker-bg': token.colorBgContainer,
    '--poker-border': token.colorBorderSecondary,
    '--poker-text': token.colorText,
    '--poker-text-secondary': token.colorTextSecondary,
    '--poker-muted': token.colorTextTertiary,
    '--poker-primary': token.colorPrimary,
    '--poker-primary-bg': token.colorPrimaryBg,
    '--poker-success': token.colorSuccess,
    '--poker-active-bg': token.controlItemBgActive,
    '--poker-hover-bg': token.controlItemBgHover,
  }

  return (
    <div style={cssVars}>
      <br />
      <Flex justify="space-between" align="center" style={{ marginBottom: 16 }}>
        <SessionHeader
          round={activeRound}
          totalParticipants={connectedParticipants.length}
          sessionName={session.name}
          sessionStatus={session.status}
          facilitatorName={session.facilitator?.name ?? 'Unknown'}
        />
        <Flex align="center" gap={12}>
          {session.estimationScale && (
            <Tag>{session.estimationScale.name}</Tag>
          )}
          <Button
            icon={<LinkOutlined />}
            onClick={handleCopyInviteLink}
          >
            Copy Invite Link
          </Button>
          {connectedParticipants.length > 0 && (
            <Avatar.Group
              max={{
                count: 8,
                style: { backgroundColor: token.colorPrimary, fontSize: 12 },
              }}
              size="small"
            >
              {connectedParticipants.map((p) => (
                <Tooltip key={p.id} title={p.name}>
                  <Avatar
                    size="small"
                    style={{ backgroundColor: token.colorPrimary }}
                  >
                    {p.name.charAt(0).toUpperCase()}
                  </Avatar>
                </Tooltip>
              ))}
            </Avatar.Group>
          )}
        </Flex>
      </Flex>
      <Row gutter={16}>
        <Col xs={24} md={17}>
          {activeRound ? (
            <VotingArea
              round={activeRound}
              sessionId={session.id}
              sessionKey={session.key}
              estimationScale={session.estimationScale}
              canManage={canManage && isActive}
              participants={roundParticipants}
            />
          ) : (
            <Empty
              description={
                isActive ? 'Add a round to get started.' : 'No rounds.'
              }
              style={{ padding: '48px 0' }}
            />
          )}
        </Col>
        <Col xs={24} md={7}>
          <SessionSidebar
            session={session}
            activeRoundId={activeRound?.id}
            onSelectRound={setSelectedRoundId}
            onRemoveRound={handleRemoveRound}
            onComplete={handleComplete}
            isCompleting={isCompleting}
            canManage={canManage}
            isActive={isActive}
          />
        </Col>
      </Row>
    </div>
  )
}

const PokerSessionDetailPageWithAuthorization = authorizePage(
  PokerSessionDetailPage,
  'Permission',
  'Permissions.PokerSessions.View',
)

export default PokerSessionDetailPageWithAuthorization
