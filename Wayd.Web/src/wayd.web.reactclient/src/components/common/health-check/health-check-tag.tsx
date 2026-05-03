'use client'

import { Descriptions, Popover, Spin, Tag } from 'antd'
import dayjs from 'dayjs'
import { MarkdownRenderer } from '../markdown'
import { healthCheckTagColor } from './health-check-utils'

const { Item } = Descriptions

export interface HealthCheckStatusTagData {
  status: { name: string }
  expiration: Date
}

export interface HealthCheckDetailsData extends HealthCheckStatusTagData {
  reportedBy: { name: string }
  reportedOn: Date
  note?: string
}

export interface HealthCheckTagProps {
  healthCheck?: HealthCheckStatusTagData | null
  details?: HealthCheckDetailsData | null
  isLoading?: boolean
  onOpenChange?: (open: boolean) => void
}

const HealthCheckTag = ({
  healthCheck,
  details,
  isLoading = false,
  onOpenChange,
}: HealthCheckTagProps) => {
  if (!healthCheck) return null

  const content = () => {
    if (isLoading) return <Spin size="small" />

    if (!details) {
      return (
        <Descriptions size="small" column={1} style={{ maxWidth: '250px' }}>
          <Item label="Expires On">
            {dayjs(healthCheck.expiration).format('M/D/YYYY hh:mm A')}
          </Item>
        </Descriptions>
      )
    }

    const maxWidth = details.note
      ? details.note.length <= 200
        ? '300px'
        : '400px'
      : '250px'

    return (
      <Descriptions size="small" column={1} style={{ maxWidth }}>
        <Item label="Reported By">{details.reportedBy.name}</Item>
        <Item label="Reported On">
          {dayjs(details.reportedOn).format('M/D/YYYY')}
        </Item>
        <Item label="Expires On">
          {dayjs(details.expiration).format('M/D/YYYY hh:mm A')}
        </Item>
        <Item>
          <MarkdownRenderer markdown={details.note} />
        </Item>
      </Descriptions>
    )
  }

  return (
    <Popover
      title="Health Check"
      content={content}
      trigger="hover"
      onOpenChange={onOpenChange}
    >
      <Tag color={healthCheckTagColor(healthCheck.status.name)}>
        {healthCheck.status.name}
      </Tag>
    </Popover>
  )
}

export default HealthCheckTag
