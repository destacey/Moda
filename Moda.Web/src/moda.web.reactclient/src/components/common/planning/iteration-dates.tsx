import dayjs from 'dayjs'
import { FC } from 'react'
import { Card, Divider, Flex, Space, Typography } from 'antd'

const { Text } = Typography

const DATE_TIME_FORMAT = 'MMM D, YYYY h:mm A'

export interface IterationDatesProps {
  start: Date | null
  end: Date | null
}

const IterationDates: FC<IterationDatesProps> = ({
  start,
  end,
}: IterationDatesProps) => {
  if (!start || !end) return null

  const startDate = dayjs(start)
  const endDate = dayjs(end)

  // For calendar day counting, ignore time and count inclusive days
  const startDay = dayjs(start).startOf('day')
  const endDay = dayjs(end).startOf('day')
  const durationDays = endDay.diff(startDay, 'day') + 1

  return (
    <Card size="small" style={{ width: 'fit-content' }}>
      <Space size="middle">
        <Flex vertical>
          <Text
            type="secondary"
            style={{ fontSize: 12, textTransform: 'uppercase' }}
          >
            Start Date
          </Text>
          <Text>{startDate.format(DATE_TIME_FORMAT)}</Text>
        </Flex>
        <Text type="secondary">→</Text>
        <Flex vertical>
          <Text
            type="secondary"
            style={{ fontSize: 12, textTransform: 'uppercase' }}
          >
            End Date
          </Text>
          <Text>{endDate.format(DATE_TIME_FORMAT)}</Text>
        </Flex>
        <Divider type="vertical" />
        <Flex vertical>
          <Text
            type="secondary"
            style={{ fontSize: 12, textTransform: 'uppercase' }}
          >
            Duration
          </Text>
          <Text>{durationDays} Days</Text>
        </Flex>
      </Space>
    </Card>
  )
}

export default IterationDates
