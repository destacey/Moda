import { PlanningIntervalListDto } from '@/src/services/moda-api'
import { daysRemaining } from '@/src/utils'
import { Card, Space, Typography } from 'antd'
import dayjs from 'dayjs'
import Link from 'next/link'

const { Text } = Typography

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
          <Text>
            ({daysRemaining(planningInterval.start)} days until start)
          </Text>
        )
      case 'Active':
        return (
          <Text>({daysRemaining(planningInterval.end)} days remaining)</Text>
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
          <Text type="secondary"> - </Text>
          {dayjs(planningInterval.end).format('M/D/YYYY')}
          <DaysCountdownLabel />
        </Space>
        <Space>
          <Link href={`/planning/planning-intervals/${planningInterval.key}`}>
            Details
          </Link>
          <Text type="secondary"> | </Text>
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
