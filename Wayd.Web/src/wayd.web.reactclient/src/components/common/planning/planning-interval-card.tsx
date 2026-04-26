import { PlanningIntervalListDto } from '@/src/services/wayd-api'
import { daysRemaining } from '@/src/utils'
import { Card, Space, Typography } from 'antd'
import dayjs from 'dayjs'
import Link from 'next/link'
import { IterationState } from '../../types'
import { useGetPlanningIntervalIterationsQuery } from '@/src/store/features/planning/planning-interval-api'

const { Text } = Typography

export interface PlanningIntervalCardProps {
  planningInterval: PlanningIntervalListDto
}

const DaysCountdownLabel = ({
  planningInterval,
}: {
  planningInterval: PlanningIntervalListDto
}) => {
  switch (planningInterval.state.id as IterationState) {
    case IterationState.Future:
      return (
        <Text>({daysRemaining(planningInterval.start)} days until start)</Text>
      )
    case IterationState.Active:
      return <Text>({daysRemaining(planningInterval.end)} days remaining)</Text>
    default:
      return null
  }
}

const PlanningIntervalCard = ({
  planningInterval,
}: PlanningIntervalCardProps) => {
  const isFuture =
    (planningInterval.state.id as IterationState) === IterationState.Future
  const primaryLinkHref = isFuture
    ? `/planning/planning-intervals/${planningInterval.key}/details`
    : `/planning/planning-intervals/${planningInterval.key}/overview`
  const primaryLinkLabel = isFuture ? 'Details' : 'Overview'

  const { data: iterationsData } = useGetPlanningIntervalIterationsQuery(
    planningInterval.key,
    {
      skip: planningInterval?.state.name !== 'Active',
    },
  )

  const activeIteration = iterationsData?.find(
    (iteration) => iteration.state === 'Active',
  )

  if (!planningInterval) return null

  return (
    <Card size="small" title={planningInterval.name}>
      <Space vertical>
        <Space wrap>
          {dayjs(planningInterval.start).format('MMM D, YYYY')}
          <Text type="secondary"> - </Text>
          {dayjs(planningInterval.end).format('MMM D, YYYY')}
          <DaysCountdownLabel planningInterval={planningInterval} />
        </Space>
        <Space>
          <Link href={primaryLinkHref}>{primaryLinkLabel}</Link>
          <Text type="secondary"> | </Text>
          <Link
            href={`/planning/planning-intervals/${planningInterval.key}/plan-review`}
          >
            Plan Review
          </Link>
          {activeIteration && (
            <>
              <Text type="secondary"> | </Text>
              <Link
                href={`/planning/planning-intervals/${planningInterval.key}/iterations/${activeIteration.key}`}
              >
                {activeIteration.name}
              </Link>
            </>
          )}
        </Space>
      </Space>
    </Card>
  )
}

export default PlanningIntervalCard
