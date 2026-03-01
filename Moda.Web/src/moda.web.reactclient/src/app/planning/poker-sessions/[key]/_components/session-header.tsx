'use client'

import { PokerRoundDto } from '@/src/services/moda-api'
import { Flex, Tag, Typography } from 'antd'
import { FC } from 'react'

const { Title, Text } = Typography

export interface SessionHeaderProps {
  round?: PokerRoundDto
  totalParticipants: number
  sessionName: string
  sessionStatus: string
  facilitatorName: string
}

const sessionStatusColor: Record<string, string> = {
  Active: 'processing',
  Completed: 'success',
}

const roundStatusColor: Record<string, string> = {
  Voting: 'processing',
  Revealed: 'warning',
  Accepted: 'success',
}

const SessionHeader: FC<SessionHeaderProps> = ({
  round,
  totalParticipants,
  sessionName,
  sessionStatus,
  facilitatorName,
}) => {
  return (
    <Flex vertical gap={4}>
      <Flex align="center" gap={12}>
        <Title level={4} style={{ margin: 0 }}>
          {sessionName}
        </Title>
        <Tag color={sessionStatusColor[sessionStatus]}>{sessionStatus}</Tag>
      </Flex>
      <Text type="secondary" style={{ fontSize: 12 }}>
        Facilitator: {facilitatorName}
      </Text>
      {round && (
        <Flex align="center" gap={8} style={{ marginTop: 8 }}>
          <Text strong style={{ fontSize: 15 }}>
            {round.label}
          </Text>
          <Tag color={roundStatusColor[round.status]}>{round.status}</Tag>
          {totalParticipants > 0 && (
            <Text type="secondary" style={{ fontSize: 13 }}>
              {round.voteCount}/{totalParticipants} voted
            </Text>
          )}
        </Flex>
      )}
    </Flex>
  )
}

export default SessionHeader
