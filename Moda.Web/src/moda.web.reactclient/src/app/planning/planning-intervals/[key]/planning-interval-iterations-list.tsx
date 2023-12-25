'use client'

import { useGetPlanningIntervalIterations } from '@/src/services/queries/planning-queries'
import { Card, Flex, List, Space } from 'antd'
import dayjs from 'dayjs'

interface PlanningIntervalIterationsListProps {
  id: string
}

const PlanningIntervalIterationsList = ({
  id,
}: PlanningIntervalIterationsListProps) => {
  const { data: iterations } = useGetPlanningIntervalIterations(id)

  if (!iterations) return null
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
              <Flex wrap="wrap">
                {dayjs(iteration.start).format('M/D/YYYY')} -{' '}
                {dayjs(iteration.end).format('M/D/YYYY')}
              </Flex>
            </List.Item>
          )}
        />
      </Card>
    </>
  )
}

export default PlanningIntervalIterationsList
