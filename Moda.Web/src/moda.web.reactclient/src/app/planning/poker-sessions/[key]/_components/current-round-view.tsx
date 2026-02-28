'use client'

import {
  EstimationScaleDetailsDto,
  PokerRoundDto,
} from '@/src/services/moda-api'
import { useMessage } from '@/src/components/contexts/messaging'
import {
  useStartPokerRoundMutation,
  useRevealPokerRoundMutation,
  useResetPokerRoundMutation,
  useSetPokerRoundConsensusMutation,
  useSubmitPokerVoteMutation,
} from '@/src/store/features/planning/poker-sessions-api'
import { Button, Card, Flex, Select, Space, Tag, Typography } from 'antd'
import { FC, useCallback, useState } from 'react'
import EstimationCardDeck from './estimation-card-deck'
import VoteResults from './vote-results'

const { Title, Text } = Typography

export interface CurrentRoundViewProps {
  round: PokerRoundDto
  sessionId: string
  sessionKey: number
  estimationScale?: EstimationScaleDetailsDto
  canManage: boolean
}

const CurrentRoundView: FC<CurrentRoundViewProps> = ({
  round,
  sessionId,
  sessionKey,
  estimationScale,
  canManage,
}) => {
  const messageApi = useMessage()
  const [selectedVote, setSelectedVote] = useState<string | undefined>()
  const [consensusValue, setConsensusValue] = useState<string | undefined>(
    round.consensusEstimate,
  )

  const [startRound, { isLoading: isStarting }] = useStartPokerRoundMutation()
  const [revealRound, { isLoading: isRevealing }] =
    useRevealPokerRoundMutation()
  const [resetRound, { isLoading: isResetting }] =
    useResetPokerRoundMutation()
  const [setConsensus, { isLoading: isSettingConsensus }] =
    useSetPokerRoundConsensusMutation()
  const [submitVote, { isLoading: isSubmitting }] =
    useSubmitPokerVoteMutation()

  const handleStart = useCallback(async () => {
    try {
      const response = await startRound({
        sessionId,
        roundId: round.id,
        sessionKey,
      })
      if (response.error) throw response.error
    } catch {
      messageApi.error('Failed to start round.')
    }
  }, [startRound, sessionId, round.id, sessionKey, messageApi])

  const handleReveal = useCallback(async () => {
    try {
      const response = await revealRound({
        sessionId,
        roundId: round.id,
        sessionKey,
      })
      if (response.error) throw response.error
    } catch {
      messageApi.error('Failed to reveal votes.')
    }
  }, [revealRound, sessionId, round.id, sessionKey, messageApi])

  const handleReset = useCallback(async () => {
    try {
      const response = await resetRound({
        sessionId,
        roundId: round.id,
        sessionKey,
      })
      if (response.error) throw response.error
      setSelectedVote(undefined)
    } catch {
      messageApi.error('Failed to reset round.')
    }
  }, [resetRound, sessionId, round.id, sessionKey, messageApi])

  const handleSetConsensus = useCallback(async () => {
    if (!consensusValue) return
    try {
      const response = await setConsensus({
        sessionId,
        roundId: round.id,
        sessionKey,
        request: { estimate: consensusValue },
      })
      if (response.error) throw response.error
      messageApi.success('Consensus set.')
    } catch {
      messageApi.error('Failed to set consensus.')
    }
  }, [
    setConsensus,
    sessionId,
    round.id,
    sessionKey,
    consensusValue,
    messageApi,
  ])

  const handleVote = useCallback(
    async (value: string) => {
      setSelectedVote(value)
      try {
        const response = await submitVote({
          sessionId,
          roundId: round.id,
          sessionKey,
          request: { value },
        })
        if (response.error) throw response.error
      } catch {
        messageApi.error('Failed to submit vote.')
      }
    },
    [submitVote, sessionId, round.id, sessionKey, messageApi],
  )

  const isVoting = round.status === 'Voting'
  const isRevealed = round.status === 'Revealed'
  const isPending = round.status === 'Pending'
  const isAccepted = round.status === 'Accepted'

  const scaleValues = estimationScale?.values ?? []
  const scaleOptions = scaleValues.map((v) => ({
    label: v.value,
    value: v.value,
  }))

  return (
    <Card>
      <Flex vertical gap={16}>
        <Flex justify="space-between" align="center">
          <div>
            <Title level={4} style={{ margin: 0 }}>
              {round.label}
            </Title>
            <Tag
              color={
                isPending
                  ? 'default'
                  : isVoting
                    ? 'processing'
                    : isRevealed
                      ? 'warning'
                      : 'success'
              }
            >
              {round.status}
            </Tag>
            {round.voteCount > 0 && (
              <Text type="secondary">
                {round.voteCount} vote{round.voteCount !== 1 ? 's' : ''}
              </Text>
            )}
          </div>
          {canManage && (
            <Space>
              {isPending && (
                <Button type="primary" onClick={handleStart} loading={isStarting}>
                  Start Voting
                </Button>
              )}
              {isVoting && (
                <Button type="primary" onClick={handleReveal} loading={isRevealing}>
                  Reveal Votes
                </Button>
              )}
              {isRevealed && (
                <>
                  <Button onClick={handleReset} loading={isResetting}>
                    Re-vote
                  </Button>
                  <Select
                    placeholder="Select consensus"
                    style={{ width: 140 }}
                    options={scaleOptions}
                    value={consensusValue}
                    onChange={setConsensusValue}
                  />
                  <Button
                    type="primary"
                    onClick={handleSetConsensus}
                    loading={isSettingConsensus}
                    disabled={!consensusValue}
                  >
                    Accept
                  </Button>
                </>
              )}
            </Space>
          )}
        </Flex>

        {isVoting && (
          <EstimationCardDeck
            values={scaleValues}
            selectedValue={selectedVote}
            onSelect={handleVote}
            disabled={isSubmitting}
          />
        )}

        {(isRevealed || isAccepted) && (
          <VoteResults votes={round.votes} isRevealed />
        )}

        {isVoting && round.voteCount > 0 && (
          <VoteResults votes={round.votes} isRevealed={false} />
        )}
      </Flex>
    </Card>
  )
}

export default CurrentRoundView
