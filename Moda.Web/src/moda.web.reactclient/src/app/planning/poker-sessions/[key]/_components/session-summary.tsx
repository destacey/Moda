'use client'

import { PokerRoundDto } from '@/src/services/moda-api'
import { Statistic } from 'antd'
import { FC, useMemo } from 'react'
import styles from './poker-session.module.css'

export interface SessionSummaryProps {
  rounds: PokerRoundDto[]
}

const SessionSummary: FC<SessionSummaryProps> = ({ rounds }) => {
  const stats = useMemo(() => {
    const accepted = rounds.filter((r) => r.status === 'Accepted')
    const numericValues = accepted
      .map((r) => parseFloat(r.consensusEstimate ?? ''))
      .filter((v) => !isNaN(v))
    const total = numericValues.reduce((sum, v) => sum + v, 0)

    return {
      completed: accepted.length,
      totalPoints: numericValues.length > 0 ? total : '-',
    }
  }, [rounds])

  return (
    <div className={styles.summaryGrid}>
      <Statistic title="Completed" value={stats.completed} />
      <Statistic title="Total Points" value={stats.totalPoints} />
    </div>
  )
}

export default SessionSummary
