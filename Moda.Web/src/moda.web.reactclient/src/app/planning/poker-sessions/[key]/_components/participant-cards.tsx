'use client'

import { PokerVoteDto } from '@/src/services/moda-api'
import { Flex } from 'antd'
import { FC, useMemo } from 'react'
import styles from './poker-session.module.css'

export interface ParticipantCardsProps {
  participants: { id: string; name: string }[]
  votes: PokerVoteDto[]
  isRevealed: boolean
}

const ParticipantCards: FC<ParticipantCardsProps> = ({
  participants,
  votes,
  isRevealed,
}) => {
  const voteMap = useMemo(() => {
    const map = new Map<string, string>()
    for (const vote of votes) {
      if (vote.participant) {
        map.set(vote.participant.id, vote.value)
      }
    }
    return map
  }, [votes])

  return (
    <Flex wrap gap={16} justify="center">
      {participants.map((p) => {
        const vote = voteMap.get(p.id)
        const hasVoted = vote !== undefined

        let cardClass = styles.participantCard
        if (isRevealed && hasVoted) {
          cardClass += ` ${styles.participantCardRevealed}`
        } else if (hasVoted) {
          cardClass += ` ${styles.participantCardVoted}`
        } else {
          cardClass += ` ${styles.participantCardPending}`
        }

        return (
          <div key={p.id} style={{ textAlign: 'center' }}>
            <div className={cardClass}>
              {isRevealed && hasVoted ? (
                <span className={styles.cardValue}>{vote}</span>
              ) : hasVoted ? (
                <span className={styles.cardSymbol}>&#9830;</span>
              ) : null}
            </div>
            <div className={styles.cardName}>{p.name}</div>
          </div>
        )
      })}
    </Flex>
  )
}

export default ParticipantCards
