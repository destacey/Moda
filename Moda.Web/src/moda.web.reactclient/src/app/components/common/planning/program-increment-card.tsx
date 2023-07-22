import { ProgramIncrementListDto } from '@/src/services/moda-api'
import { Card, Space, Typography } from 'antd'
import dayjs from 'dayjs'
import Link from 'next/link'

export interface ProgramIncrementCardProps {
  programIncrement: ProgramIncrementListDto
}

const ProgramIncrementCard = ({
  programIncrement,
}: ProgramIncrementCardProps) => {
  const start = new Date(programIncrement.start)
  const end = new Date(programIncrement.end)
  const now = new Date()
  const daysRemaining = Math.ceil(
    (end.getTime() - now.getTime()) / (1000 * 3600 * 24)
  )
  return (
    <Card size="small" title={programIncrement.name}>
      <Space direction="vertical">
        <Space wrap>
          {dayjs(programIncrement.start).format('M/D/YYYY')}
          <Typography.Text type="secondary"> - </Typography.Text>
          {dayjs(programIncrement.end).format('M/D/YYYY')}
          <Typography.Text> ({daysRemaining} days remaining)</Typography.Text>
        </Space>
        <Space>
          <Link
            href={`/planning/program-increments/${programIncrement.localId}`}
          >
            Details
          </Link>
          <Typography.Text type="secondary"> | </Typography.Text>
          <Link
            href={`/planning/program-increments/${programIncrement.localId}/plan-review`}
          >
            Plan Review
          </Link>
        </Space>
      </Space>
    </Card>
  )
}

export default ProgramIncrementCard
