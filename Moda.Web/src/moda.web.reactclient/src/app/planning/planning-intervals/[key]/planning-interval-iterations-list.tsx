'use client'

import { useGetPlanningIntervalCalendar } from '@/src/services/queries/planning-queries'
import { Card, Flex, List, Space } from 'antd'
import Typography from 'antd/es/typography/Typography'
import dayjs from 'dayjs'

interface PlanningIntervalIterationsListProps {
  id: string
}

const PlanningIntervalIterationsList = ({
  id,
}: PlanningIntervalIterationsListProps) => {
  const { data: calendar } = useGetPlanningIntervalCalendar(id)

  if (!calendar) return null
  return (
    <>
      <Card size="small" title="Iterations">
        <List
          size="small"
          itemLayout="horizontal"
          dataSource={calendar.iterationSchedules}
          renderItem={(iteration) => (
            <List.Item>
              <List.Item.Meta title={iteration.name} />
              <Flex wrap="wrap">
                {dayjs(iteration.dateRange.start).format('M/D/YYYY')} -{' '}
                {dayjs(iteration.dateRange.end).format('M/D/YYYY')}
              </Flex>
            </List.Item>
          )}
        />
      </Card>
    </>
  )
}

export default PlanningIntervalIterationsList
