'use client'

import { PokerRoundDto } from '@/src/services/moda-api'
import { useMessage } from '@/src/components/contexts/messaging'
import { useAddPokerRoundMutation } from '@/src/store/features/planning/poker-sessions-api'
import {
  CheckCircleOutlined,
  DeleteOutlined,
  EyeOutlined,
  PlayCircleOutlined,
  PlusOutlined,
} from '@ant-design/icons'
import { Button, Flex, Input, Popconfirm, Tag, Typography } from 'antd'
import { FC, useCallback, useState } from 'react'
import styles from './poker-session.module.css'

const { Text } = Typography

export interface EstimationHistoryProps {
  rounds: PokerRoundDto[]
  activeRoundId?: string
  onSelectRound: (roundId: string) => void
  onRemoveRound: (roundId: string) => void
  canManage: boolean
  sessionId: string
  sessionKey: number
  isActive: boolean
}

const statusIcon: Record<string, React.ReactNode> = {
  Voting: <PlayCircleOutlined />,
  Revealed: <EyeOutlined />,
  Accepted: <CheckCircleOutlined />,
}

const EstimationHistory: FC<EstimationHistoryProps> = ({
  rounds,
  activeRoundId,
  onSelectRound,
  onRemoveRound,
  canManage,
  sessionId,
  sessionKey,
  isActive,
}) => {
  const messageApi = useMessage()
  const [addRound, { isLoading: isAdding }] = useAddPokerRoundMutation()
  const [newRoundLabel, setNewRoundLabel] = useState('')

  const handleAddRound = useCallback(async () => {
    const label = newRoundLabel.trim()
    if (!label) return
    try {
      const response = await addRound({
        sessionId,
        sessionKey,
        request: { label },
      })
      if (response.error) throw response.error
      setNewRoundLabel('')
    } catch {
      messageApi.error('Failed to add round.')
    }
  }, [addRound, sessionId, sessionKey, newRoundLabel, messageApi])

  return (
    <div>
      <div className={styles.historyList}>
        {rounds.map((round) => {
          const isActiveItem = round.id === activeRoundId
          const itemClass = `${styles.historyItem}${isActiveItem ? ` ${styles.historyItemActive}` : ''}`

          return (
            <Flex
              key={round.id}
              className={itemClass}
              align="center"
              justify="space-between"
              onClick={() => onSelectRound(round.id)}
            >
              <Flex vertical style={{ minWidth: 0, flex: 1 }}>
                <Text
                  ellipsis={{ tooltip: round.label }}
                  style={{ fontSize: 13 }}
                >
                  {round.label}
                </Text>
                <Flex gap={4} align="center">
                  <span style={{ fontSize: 12 }}>
                    {statusIcon[round.status]}
                  </span>
                  {round.consensusEstimate && (
                    <Tag color="green" style={{ margin: 0, fontSize: 11 }}>
                      {round.consensusEstimate}
                    </Tag>
                  )}
                </Flex>
              </Flex>
              {canManage && (
                <Popconfirm
                  title="Remove this round?"
                  onConfirm={(e) => {
                    e?.stopPropagation()
                    onRemoveRound(round.id)
                  }}
                  onCancel={(e) => e?.stopPropagation()}
                >
                  <Button
                    type="text"
                    size="small"
                    danger
                    icon={<DeleteOutlined />}
                    onClick={(e) => e.stopPropagation()}
                  />
                </Popconfirm>
              )}
            </Flex>
          )
        })}
      </div>
      {canManage && isActive && (
        <Flex gap={8} style={{ padding: '8px 12px' }}>
          <Input
            size="small"
            placeholder="Add a round..."
            value={newRoundLabel}
            onChange={(e) => setNewRoundLabel(e.target.value)}
            onPressEnter={handleAddRound}
            maxLength={512}
          />
          <Button
            size="small"
            type="text"
            icon={<PlusOutlined />}
            onClick={handleAddRound}
            loading={isAdding}
            disabled={!newRoundLabel.trim()}
          />
        </Flex>
      )}
    </div>
  )
}

export default EstimationHistory
