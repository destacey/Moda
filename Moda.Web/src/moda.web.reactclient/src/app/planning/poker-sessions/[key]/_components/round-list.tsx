'use client'

import { PokerRoundDto } from '@/src/services/moda-api'
import {
  CheckCircleOutlined,
  ClockCircleOutlined,
  DeleteOutlined,
  EyeOutlined,
  PlayCircleOutlined,
} from '@ant-design/icons'
import { Button, List, Popconfirm, Tag, Tooltip, Typography } from 'antd'
import { FC } from 'react'

const { Text } = Typography

export interface RoundListProps {
  rounds: PokerRoundDto[]
  activeRoundId?: string
  onSelectRound: (roundId: string) => void
  onRemoveRound: (roundId: string) => void
  canManage: boolean
}

const statusColor: Record<string, string> = {
  Pending: 'default',
  Voting: 'processing',
  Revealed: 'warning',
  Accepted: 'success',
}

const statusIcon: Record<string, React.ReactNode> = {
  Pending: <ClockCircleOutlined />,
  Voting: <PlayCircleOutlined />,
  Revealed: <EyeOutlined />,
  Accepted: <CheckCircleOutlined />,
}

const RoundList: FC<RoundListProps> = ({
  rounds,
  activeRoundId,
  onSelectRound,
  onRemoveRound,
  canManage,
}) => {
  return (
    <List
      size="small"
      dataSource={rounds}
      renderItem={(round) => (
        <List.Item
          style={{
            cursor: 'pointer',
            background: round.id === activeRoundId ? '#f0f5ff' : undefined,
            padding: '8px 12px',
          }}
          onClick={() => onSelectRound(round.id)}
          actions={
            canManage && round.status !== 'Voting'
              ? [
                  <Popconfirm
                    key="delete"
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
                  </Popconfirm>,
                ]
              : undefined
          }
        >
          <List.Item.Meta
            title={
              <Text
                ellipsis={{ tooltip: round.label }}
                style={{ maxWidth: 200 }}
              >
                {round.label}
              </Text>
            }
            description={
              <Tooltip title={round.status}>
                <Tag
                  color={statusColor[round.status]}
                  icon={statusIcon[round.status]}
                >
                  {round.status}
                </Tag>
                {round.consensusEstimate && (
                  <Tag color="green">{round.consensusEstimate}</Tag>
                )}
              </Tooltip>
            }
          />
        </List.Item>
      )}
    />
  )
}

export default RoundList
