'use client'

import { Descriptions, Popover, Spin, Tag } from 'antd'
import { FlagFilled } from '@ant-design/icons'
import dayjs from 'dayjs'
import { MarkdownRenderer } from '../markdown'
import { healthCheckTagColor } from './health-check-utils'

const { Item } = Descriptions

export type HealthCheckVariant = 'tag' | 'flag'

export interface HealthCheckStatusTagData {
  status: { name: string }
  expiration?: Date
  reportedOn?: Date
  note?: string
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
  variant?: HealthCheckVariant
  onOpenChange?: (open: boolean) => void
}

const statusToColorVar = (status: string): string => {
  switch (status) {
    case 'Healthy':
      return 'var(--ant-color-success)'
    case 'At Risk':
      return 'var(--ant-color-warning)'
    case 'Unhealthy':
      return 'var(--ant-color-error)'
    default:
      return 'var(--ant-color-text-disabled)'
  }
}

const HealthCheckTag = ({
  healthCheck,
  details,
  isLoading = false,
  variant = 'tag',
  onOpenChange,
}: HealthCheckTagProps) => {
  if (!healthCheck) return null

  const content = () => {
    if (isLoading) return <Spin size="small" />

    if (!details) {
      return (
        <Descriptions size="small" column={1} style={{ maxWidth: '250px' }}>
          {healthCheck.expiration && (
            <Item label="Expires On">
              {dayjs(healthCheck.expiration).format('M/D/YYYY hh:mm A')}
            </Item>
          )}
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

  const trigger =
    variant === 'flag' ? (
      <FlagFilled
        style={{ color: statusToColorVar(healthCheck.status.name), fontSize: 14 }}
        aria-label={`Health: ${healthCheck.status.name}`}
      />
    ) : (
      <Tag color={healthCheckTagColor(healthCheck.status.name)}>
        {healthCheck.status.name}
      </Tag>
    )

  return (
    <Popover
      title="Health Check"
      content={content}
      trigger="hover"
      onOpenChange={onOpenChange}
    >
      {trigger}
    </Popover>
  )
}

export default HealthCheckTag
