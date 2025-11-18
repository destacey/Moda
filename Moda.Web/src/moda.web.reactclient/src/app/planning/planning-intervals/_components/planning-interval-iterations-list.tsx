'use client'

import ModaDateRange from '@/src/components/common/moda-date-range'
import { useGetPlanningIntervalIterationsQuery } from '@/src/store/features/planning/planning-interval-api'
import { Card, List } from 'antd'
import { FC } from 'react'
import useTheme from '@/src/components/contexts/theme'

const { Item } = List
const { Meta } = Item

interface PlanningIntervalIterationsListProps {
  planningIntervalKey: number
}

const PlanningIntervalIterationsList: FC<
  PlanningIntervalIterationsListProps
> = (props) => {
  const { planningIntervalKey } = props
  const { token } = useTheme()
  const { data: iterations } = useGetPlanningIntervalIterationsQuery(
    planningIntervalKey,
    { skip: !planningIntervalKey },
  )

  const isActiveIteration = (iteration: any) => {
    const today = new Date()
    const start = new Date(iteration.start)
    const end = new Date(iteration.end)
    return today >= start && today <= end
  }

  if (!iterations || iterations.length === 0) return null
  return (
    <>
      <Card size="small" title="Iterations">
        <List
          size="small"
          itemLayout="horizontal"
          dataSource={iterations}
          renderItem={(iteration) => {
            const isActive = isActiveIteration(iteration)
            return (
              <Item
                style={{
                  backgroundColor: isActive ? token.colorPrimaryBg : undefined,
                }}
              >
                <Meta
                  title={iteration.name}
                  description={iteration.category.name}
                  style={{
                    color: isActive ? token.colorPrimaryText : undefined,
                  }}
                />
                <ModaDateRange dateRange={iteration} />
              </Item>
            )
          }}
        />
      </Card>
    </>
  )
}

export default PlanningIntervalIterationsList
