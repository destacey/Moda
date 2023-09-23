import { ProgramIncrementListDto } from '@/src/services/moda-api'
import { programIncrementDaysRemaining } from '@/src/utils'
import { Card, Space, Typography } from 'antd'
import dayjs from 'dayjs'
import Link from 'next/link'

export interface ProgramIncrementCardProps {
  programIncrement: ProgramIncrementListDto
}

const ProgramIncrementCard = ({
  programIncrement,
}: ProgramIncrementCardProps) => {
  const daysRemaining = programIncrementDaysRemaining(programIncrement.end)

  const start = new Date(programIncrement.start)
  const now = new Date()
  const daysUntilStart = Math.ceil(
    (start.getTime() - now.getTime()) / (1000 * 3600 * 24),
  )

  const daysRemainingText = () => {
    if (programIncrement.state === 'Active') {
      return `${daysRemaining} days remaining`
    }

    if (programIncrement.state === 'Future') {
      return `${daysUntilStart} days until start`
    }
  }
  return (
    <Card size="small" title={programIncrement.name}>
      <Space direction="vertical">
        <Space wrap>
          {dayjs(programIncrement.start).format('M/D/YYYY')}
          <Typography.Text type="secondary"> - </Typography.Text>
          {dayjs(programIncrement.end).format('M/D/YYYY')}
          <Typography.Text> ({daysRemainingText()})</Typography.Text>
        </Space>
        <Space>
          <Link href={`/planning/program-increments/${programIncrement.key}`}>
            Details
          </Link>
          <Typography.Text type="secondary"> | </Typography.Text>
          <Link
            href={`/planning/program-increments/${programIncrement.key}/plan-review`}
          >
            Plan Review
          </Link>
        </Space>
      </Space>
    </Card>
  )
}

export default ProgramIncrementCard
