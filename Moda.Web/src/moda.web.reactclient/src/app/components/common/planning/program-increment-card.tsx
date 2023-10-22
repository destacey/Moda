import { ProgramIncrementListDto } from '@/src/services/moda-api'
import { daysRemaining } from '@/src/utils'
import { Card, Space, Typography } from 'antd'
import dayjs from 'dayjs'
import Link from 'next/link'

export interface ProgramIncrementCardProps {
  programIncrement: ProgramIncrementListDto
}

const ProgramIncrementCard = ({
  programIncrement,
}: ProgramIncrementCardProps) => {
  if (!programIncrement) return null

  const DaysCountdownLabel = () => {
    switch (programIncrement.state) {
      case 'Future':
        return (
          <Typography.Text>
            ({daysRemaining(programIncrement.start)} days until start)
          </Typography.Text>
        )
      case 'Active':
        return (
          <Typography.Text>
            ({daysRemaining(programIncrement.end)} days remaining)
          </Typography.Text>
        )
      default:
        return null
    }
  }

  return (
    <Card size="small" title={programIncrement.name}>
      <Space direction="vertical">
        <Space wrap>
          {dayjs(programIncrement.start).format('M/D/YYYY')}
          <Typography.Text type="secondary"> - </Typography.Text>
          {dayjs(programIncrement.end).format('M/D/YYYY')}
          <DaysCountdownLabel />
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
