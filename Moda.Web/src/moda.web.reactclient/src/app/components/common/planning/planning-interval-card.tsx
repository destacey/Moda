import { PlanningIntervalListDto } from '@/src/services/moda-api'
import { daysRemaining } from '@/src/utils'
import { Card, Space, Typography } from 'antd'
import dayjs from 'dayjs'
import Link from 'next/link'

export interface PlanningIntervalCardProps {
  planningInterval: PlanningIntervalListDto
}

const PlanningIntervalCard = ({
  planningInterval,
}: PlanningIntervalCardProps) => {
  if (!planningInterval) return null

  const DaysCountdownLabel = () => {
    switch (planningInterval.state) {
      case 'Future':
        return (
          <Typography.Text>
            ({daysRemaining(planningInterval.start)} days until start)
          </Typography.Text>
        )
      case 'Active':
        return (
          <Typography.Text>
            ({daysRemaining(planningInterval.end)} days remaining)
          </Typography.Text>
        )
      default:
        return null
    }
  }

  return (
    <Card size="small" title={planningInterval.name}>
      <Space direction="vertical">
        <Space wrap>
          {dayjs(planningInterval.start).format('M/D/YYYY')}
          <Typography.Text type="secondary"> - </Typography.Text>
          {dayjs(planningInterval.end).format('M/D/YYYY')}
          <DaysCountdownLabel />
        </Space>
        <Space>
          <Link href={`/planning/planning-intervals/${planningInterval.key}`}>
            Details
          </Link>
          <Typography.Text type="secondary"> | </Typography.Text>
          <Link
            href={`/planning/planning-intervals/${planningInterval.key}/plan-review`}
          >
            Plan Review
          </Link>
        </Space>
      </Space>
    </Card>
  )
}

export default PlanningIntervalCard
