'use client'

import { VoteDto } from '@/src/services/moda-api'
import { Table } from 'antd'
import { FC, useMemo } from 'react'

export interface VoteResultsProps {
  votes: VoteDto[]
  isRevealed: boolean
}

const VoteResults: FC<VoteResultsProps> = ({ votes, isRevealed }) => {
  const columns = useMemo(
    () => [
      {
        title: 'Participant',
        dataIndex: 'participantName',
        key: 'participantName',
      },
      {
        title: 'Vote',
        dataIndex: 'value',
        key: 'value',
        render: (value: string) => (isRevealed ? value : '?'),
      },
    ],
    [isRevealed],
  )

  return (
    <Table
      size="small"
      columns={columns}
      dataSource={votes}
      rowKey="id"
      pagination={false}
    />
  )
}

export default VoteResults
