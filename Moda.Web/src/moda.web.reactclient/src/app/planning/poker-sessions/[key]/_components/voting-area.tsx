'use client'

import {
  EstimationScaleDetailsDto,
  PokerRoundDto,
} from '@/src/services/moda-api'
import { useMessage } from '@/src/components/contexts/messaging'
import {
  useRevealPokerRoundMutation,
  useResetPokerRoundMutation,
  useSetPokerRoundConsensusMutation,
  useSubmitPokerVoteMutation,
} from '@/src/store/features/planning/poker-sessions-api'
import { Button, Divider, Flex, Select, Space, Tag } from 'antd'
import { FC, useCallback, useState } from 'react'
import ParticipantCards from './participant-cards'
import EstimationCardDeck from './estimation-card-deck'
import styles from './poker-session.module.css'

export interface VotingAreaProps {
  round: PokerRoundDto
  sessionId: string
  sessionKey: number
  estimationScale?: EstimationScaleDetailsDto
  canManage: boolean
  participants: { id: string; name: string }[]
}

const VotingArea: FC<VotingAreaProps> = ({
  round,
  sessionId,
  sessionKey,
  estimationScale,
  canManage,
  participants,
}) => {
  const messageApi = useMessage()
  const [selectedVote, setSelectedVote] = useState<string | undefined>()
  const [consensusValue, setConsensusValue] = useState<string | undefined>(
    round.consensusEstimate,
  )

  const [revealRound, { isLoading: isRevealing }] =
    useRevealPokerRoundMutation()
  const [resetRound, { isLoading: isResetting }] =
    useResetPokerRoundMutation()
  const [setConsensus, { isLoading: isSettingConsensus }] =
    useSetPokerRoundConsensusMutation()
  const [submitVote, { isLoading: isSubmitting }] =
    useSubmitPokerVoteMutation()

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
  const isAccepted = round.status === 'Accepted'

  const scaleValues = estimationScale?.values ?? []
  const scaleOptions = scaleValues.map((v) => ({
    label: v,
    value: v,
  }))

  return (
    <Flex vertical gap={16} align="center" style={{ padding: '24px 0' }}>
      <ParticipantCards
        participants={participants}
        votes={round.votes}
        isRevealed={isRevealed || isAccepted}
      />

      <div className={styles.actionArea}>
        {canManage && isVoting && (
          <Button
            type="primary"
            size="large"
            onClick={handleReveal}
            loading={isRevealing}
          >
            Reveal Cards
          </Button>
        )}
        {canManage && isRevealed && (
          <Space>
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
          </Space>
        )}
        {isAccepted && round.consensusEstimate && (
          <Tag color="green" style={{ fontSize: 16, padding: '4px 12px' }}>
            Consensus: {round.consensusEstimate}
          </Tag>
        )}
      </div>

      {isVoting && (
        <>
          <Divider style={{ margin: '8px 0' }}>
            <span className={styles.yourEstimateLabel}>Your Estimate</span>
          </Divider>
          <EstimationCardDeck
            values={scaleValues}
            selectedValue={selectedVote}
            onSelect={handleVote}
            disabled={isSubmitting}
          />
        </>
      )}
    </Flex>
  )
}

export default VotingArea
