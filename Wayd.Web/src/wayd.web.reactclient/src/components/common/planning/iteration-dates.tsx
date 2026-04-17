import dayjs from 'dayjs'
import { CSSProperties, FC } from 'react'
import { Card, Flex, Space, Typography } from 'antd'

const { Text } = Typography

const DATE_TIME_FORMAT = 'MMM D, YYYY h:mm A'

export interface IterationDatesProps {
  start: Date | null
  end: Date | null
  showDurationDays?: boolean
  style?: CSSProperties
  dateFormat?: string
}

const IterationDates: FC<IterationDatesProps> = ({
  start,
  end,
  showDurationDays = true,
  style,
  dateFormat = DATE_TIME_FORMAT,
}: IterationDatesProps) => {
  if (!start || !end) return null

  const startDate = dayjs(start)
  const endDate = dayjs(end)

  // For calendar day counting, ignore time and count inclusive days
  const startDay = dayjs(start).startOf('day')
  const endDay = dayjs(end).startOf('day')
  const durationDays = endDay.diff(startDay, 'day') + 1

  return (
    <Card size="small" style={{ width: 'fit-content', ...style }}>
      <Space size="middle">
        <Flex vertical>
          <Text
            type="secondary"
            style={{ fontSize: 12, textTransform: 'uppercase' }}
          >
            Start Date
          </Text>
          <Text>{startDate.format(dateFormat)}</Text>
        </Flex>
        <Text type="secondary">→</Text>
        <Flex vertical>
          <Text
            type="secondary"
            style={{ fontSize: 12, textTransform: 'uppercase' }}
          >
            End Date
          </Text>
          <Text>{endDate.format(dateFormat)}</Text>
        </Flex>
        {showDurationDays && (
          <Flex vertical>
            <Text
              type="secondary"
              style={{ fontSize: 12, textTransform: 'uppercase' }}
            >
              Duration
            </Text>
            <Text>{durationDays} Days</Text>
          </Flex>
        )}
      </Space>
    </Card>
  )
}

export default IterationDates
