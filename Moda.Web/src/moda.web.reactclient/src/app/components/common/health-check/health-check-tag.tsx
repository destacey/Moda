import { PlanningHealthCheckDto } from '@/src/services/moda-api'
import { useGetHealthCheckById } from '@/src/services/queries/health-check-queries'
import { Descriptions, Popover, Space, Spin, Tag } from 'antd'
import dayjs from 'dayjs'
import { useState } from 'react'
import ReactMarkdown from 'react-markdown'

export interface HealthCheckTagProps {
  healthCheck?: PlanningHealthCheckDto
}

const HealthCheckTag = ({ healthCheck }: HealthCheckTagProps) => {
  const [enableQuery, setEnableQuery] = useState<boolean>(false)
  const [hovered, setHovered] = useState(false)

  const { data: healthCheckData, isLoading } = useGetHealthCheckById(
    healthCheck?.id,
    true,
  )

  if (!healthCheck) return null

  const handleHoverChange = (open: boolean) => {
    setHovered(open)
    if (open && !enableQuery) {
      setEnableQuery(true)
    }
  }

  const content = () => {
    if (!hovered) return null
    if (isLoading) return <Spin size="small" />

    const maxWidth = healthCheckData.note
      ? healthCheckData.note.length <= 200
        ? '300px'
        : '400px'
      : '250px'

    return (
      <Descriptions
        size="small"
        column={1}
        title="Health Check"
        style={{ maxWidth: maxWidth }}
      >
        <Descriptions.Item label="Reported By">
          {healthCheckData.reportedBy.name}
        </Descriptions.Item>
        <Descriptions.Item label="Reported On">
          {dayjs(healthCheckData.reportedOn).format('M/D/YYYY')}
        </Descriptions.Item>
        <Descriptions.Item label="Expires On">
          {dayjs(healthCheckData.expiration).format('M/D/YYYY hh:mm A')}
        </Descriptions.Item>
        <Descriptions.Item label="Note">
          <Space direction="vertical">
            <ReactMarkdown>{healthCheckData.note}</ReactMarkdown>
          </Space>
        </Descriptions.Item>
      </Descriptions>
    )
  }

  const color = () => {
    switch (healthCheck.status.name) {
      case 'Healthy':
        return 'success'
      case 'At Risk':
        return 'warning'
      case 'Unhealthy':
        return 'error'
      default:
        return 'default'
    }
  }

  return (
    <Popover
      content={content}
      trigger="hover"
      open={hovered}
      onOpenChange={handleHoverChange}
    >
      <Tag color={color()}>{healthCheck.status.name}</Tag>
    </Popover>
  )
}

export default HealthCheckTag
