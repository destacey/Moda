'use client'

import { PlanningHealthCheckDto } from '@/src/services/moda-api'
import { Descriptions, Popover, Space, Spin, Tag } from 'antd'
import dayjs from 'dayjs'
import ReactMarkdown from 'react-markdown'
import { useAppDispatch, useAppSelector } from '@/src/app/hooks'
import {
  getHealthCheck,
  selectHealthCheckContext,
} from '@/src/store/features/health-check-slice'
import { healthCheckTagColor } from './health-check-utils'

const { Item } = Descriptions

export interface HealthCheckTagProps {
  healthCheck?: PlanningHealthCheckDto
}

const HealthCheckTag = ({ healthCheck }: HealthCheckTagProps) => {
  const dispatch = useAppDispatch()
  const { item: healthCheckData, isLoading } = useAppSelector(
    selectHealthCheckContext,
  )

  if (!healthCheck) return null

  const handleHoverChange = (open: boolean) => {
    !!open && dispatch(getHealthCheck(healthCheck.id))
  }

  const content = () => {
    if (!healthCheckData) return null
    if (isLoading) return <Spin size="small" />

    const maxWidth = healthCheckData.note
      ? healthCheckData.note.length <= 200
        ? '300px'
        : '400px'
      : '250px'

    return (
      <Descriptions size="small" column={1} style={{ maxWidth: maxWidth }}>
        <Item label="Reported By">{healthCheckData.reportedBy.name}</Item>
        <Item label="Reported On">
          {dayjs(healthCheckData.reportedOn).format('M/D/YYYY')}
        </Item>
        <Item label="Expires On">
          {dayjs(healthCheckData.expiration).format('M/D/YYYY hh:mm A')}
        </Item>
        <Item>
          <Space direction="vertical">
            <ReactMarkdown>{healthCheckData.note}</ReactMarkdown>
          </Space>
        </Item>
      </Descriptions>
    )
  }

  return (
    <Popover
      title="Health Check"
      content={content}
      trigger="hover"
      onOpenChange={handleHoverChange}
    >
      <Tag color={healthCheckTagColor(healthCheck.status.name)}>
        {healthCheck.status.name}
      </Tag>
    </Popover>
  )
}

export default HealthCheckTag
