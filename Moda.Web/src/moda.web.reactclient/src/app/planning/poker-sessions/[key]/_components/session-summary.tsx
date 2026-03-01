'use client'

import { PokerRoundDto } from '@/src/services/moda-api'
import { Flex, Statistic } from 'antd'
import { FC, useMemo } from 'react'

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
    const avg =
      numericValues.length > 0 ? (total / numericValues.length).toFixed(1) : '-'

    return {
      estimated: `${accepted.length}/${rounds.length}`,
      totalPoints: numericValues.length > 0 ? total : '-',
      avgEstimate: avg,
    }
  }, [rounds])

  return (
    <Flex gap={16} justify="space-around" style={{ padding: '12px 0' }}>
      <Statistic title="Estimated" value={stats.estimated} />
      <Statistic title="Total Points" value={stats.totalPoints} />
      <Statistic title="Avg Estimate" value={stats.avgEstimate} />
    </Flex>
  )
}

export default SessionSummary
