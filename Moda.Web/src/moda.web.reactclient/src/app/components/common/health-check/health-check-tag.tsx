import { PlanningHealthCheckDto } from '@/src/services/moda-api'
import { Tag } from 'antd'

export interface HealthCheckTagProps {
  healthCheck?: PlanningHealthCheckDto
}

const HealthCheckTag = ({ healthCheck }: HealthCheckTagProps) => {
  if (!healthCheck) return null

  const color = () => {
    switch (healthCheck.status.name) {
      case 'Healthy':
        return 'green'
      case 'At Risk':
        return 'amber'
      case 'Unhealthy':
        return 'red'
      default:
        return 'default'
    }
  }

  return <Tag color={color()}>{healthCheck.status.name}</Tag>
}

export default HealthCheckTag
