'use client'

import ModaDateRange from '@/src/app/components/common/moda-date-range'
import { useGetPlanningIntervalIterations } from '@/src/services/queries/planning-queries'
import { Card, List } from 'antd'

interface PlanningIntervalIterationsListProps {
  id: string
}

const PlanningIntervalIterationsList = ({
  id,
}: PlanningIntervalIterationsListProps) => {
  const { data: iterations } = useGetPlanningIntervalIterations(id)

  if (!iterations || iterations.length == 0) return null
  return (
    <>
      <Card size="small" title="Iterations">
        <List
          size="small"
          itemLayout="horizontal"
          dataSource={iterations}
          renderItem={(iteration) => (
            <List.Item>
              <List.Item.Meta
                title={iteration.name}
                description={iteration.type.name}
              />
              <ModaDateRange dateRange={iteration} />
            </List.Item>
          )}
        />
      </Card>
    </>
  )
}

export default PlanningIntervalIterationsList
