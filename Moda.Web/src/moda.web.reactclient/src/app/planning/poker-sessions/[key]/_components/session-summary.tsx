'use client'

import { PokerRoundDto } from '@/src/services/moda-api'
import { Statistic } from 'antd'
import { FC, useMemo } from 'react'
import styles from './poker-session.module.css'

export interface SessionSummaryProps {
  rounds: PokerRoundDto[]
}

const SessionSummary: FC<SessionSummaryProps> = ({ rounds }) => {
  const completedCount = useMemo(
    () => rounds.filter((r) => r.status === 'Accepted').length,
    [rounds],
  )

  return (
    <div className={styles.summaryGrid}>
      <Statistic title="Completed" value={`${completedCount} / ${rounds.length}`} />
    </div>
  )
}

export default SessionSummary
