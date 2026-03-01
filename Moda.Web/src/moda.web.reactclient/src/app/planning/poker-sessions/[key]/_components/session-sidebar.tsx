'use client'

import { PokerSessionDetailsDto } from '@/src/services/moda-api'
import { Button, Card, Divider } from 'antd'
import { FC } from 'react'
import SessionSummary from './session-summary'
import EstimationHistory from './estimation-history'

export interface SessionSidebarProps {
  session: PokerSessionDetailsDto
  activeRoundId?: string
  onSelectRound: (roundId: string) => void
  onRemoveRound: (roundId: string) => void
  onComplete: () => void
  isCompleting: boolean
  canManage: boolean
  isActive: boolean
}

const SessionSidebar: FC<SessionSidebarProps> = ({
  session,
  activeRoundId,
  onSelectRound,
  onRemoveRound,
  onComplete,
  isCompleting,
  canManage,
  isActive,
}) => {
  const rounds = session.rounds ?? []

  return (
    <Card size="small" title="Session Summary" styles={{ body: { padding: 0 } }}>
      <div style={{ padding: '0 16px' }}>
        <SessionSummary rounds={rounds} />
      </div>
      <Divider style={{ margin: '0' }} />
      <div style={{ padding: '8px 0' }}>
        <div
          style={{
            padding: '0 16px 4px',
            fontSize: 12,
            fontWeight: 600,
            textTransform: 'uppercase',
            letterSpacing: '0.05em',
            opacity: 0.6,
          }}
        >
          Estimation History
        </div>
        <EstimationHistory
          rounds={rounds}
          activeRoundId={activeRoundId}
          onSelectRound={onSelectRound}
          onRemoveRound={onRemoveRound}
          canManage={canManage}
          sessionId={session.id}
          sessionKey={session.key}
          isActive={isActive}
        />
      </div>
      {canManage && isActive && (
        <>
          <Divider style={{ margin: 0 }} />
          <div style={{ padding: '12px 16px' }}>
            <Button
              danger
              block
              onClick={onComplete}
              loading={isCompleting}
            >
              Complete Session
            </Button>
          </div>
        </>
      )}
    </Card>
  )
}

export default SessionSidebar
