'use client'

import ModaDateRange from '@/src/components/common/moda-date-range'
import { useGetPlanningIntervalIterationsQuery } from '@/src/store/features/planning/planning-interval-api'
import { Card, List } from 'antd'
import { FC } from 'react'

const { Item } = List
const { Meta } = Item

interface PlanningIntervalIterationsListProps {
  planningIntervalKey: number
}

const PlanningIntervalIterationsList: FC<
  PlanningIntervalIterationsListProps
> = (props) => {
  const { planningIntervalKey } = props
  const { data: iterations } = useGetPlanningIntervalIterationsQuery(
    planningIntervalKey,
    { skip: !planningIntervalKey },
  )

  if (!iterations || iterations.length == 0) return null
  return (
    <>
      <Card size="small" title="Iterations">
        <List
          size="small"
          itemLayout="horizontal"
          dataSource={iterations}
          renderItem={(iteration) => (
            <Item>
              <Meta
                title={iteration.name}
                description={iteration.category.name}
              />
              <ModaDateRange dateRange={iteration} />
            </Item>
          )}
        />
      </Card>
    </>
  )
}

export default PlanningIntervalIterationsList
