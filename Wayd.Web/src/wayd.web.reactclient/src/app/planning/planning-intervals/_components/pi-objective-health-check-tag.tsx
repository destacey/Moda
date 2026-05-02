'use client'

import { PlanningHealthCheckDto } from '@/src/services/wayd-api'
import { Descriptions, Popover, Spin, Tag } from 'antd'
import dayjs from 'dayjs'
import { useState } from 'react'
import { useGetObjectiveHealthCheckQuery } from '@/src/store/features/planning/pi-objective-health-checks-api'
import { healthCheckTagColor } from '../../../../components/common/health-check/health-check-utils'
import { MarkdownRenderer } from '../../../../components/common/markdown'

const { Item } = Descriptions

export interface PiObjectiveHealthCheckTagProps {
  healthCheck?: PlanningHealthCheckDto
  /**
   * Parent context required to resolve the health check via the nested
   * /planning-intervals/{id}/objectives/{objectiveId}/health-checks endpoint.
   * Required when the popover should fetch full health-check details on hover.
   */
  planningIntervalId?: string
  objectiveId?: string
}

const PiObjectiveHealthCheckTag = ({
  healthCheck,
  planningIntervalId,
  objectiveId,
}: PiObjectiveHealthCheckTagProps) => {
  const [hovered, setHovered] = useState(false)
  const canFetchDetails = !!planningIntervalId && !!objectiveId

  // Skip the request until the user actually hovers — avoids hitting the API
  // for every tag rendered in a long list of objectives. RTK Query caches per
  // arg-tuple, so re-hovering the same tag is free.
  const { data: healthCheckData, isLoading } = useGetObjectiveHealthCheckQuery(
    {
      planningIntervalId: planningIntervalId ?? '',
      objectiveId: objectiveId ?? '',
      healthCheckId: healthCheck?.id ?? '',
    },
    { skip: !hovered || !canFetchDetails || !healthCheck },
  )

  if (!healthCheck) return null

  const content = () => {
    if (!canFetchDetails) {
      return (
        <Descriptions size="small" column={1} style={{ maxWidth: '250px' }}>
          <Item label="Expires On">
            {dayjs(healthCheck.expiration).format('M/D/YYYY hh:mm A')}
          </Item>
        </Descriptions>
      )
    }
    if (isLoading || !healthCheckData) return <Spin size="small" />

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
          <MarkdownRenderer markdown={healthCheckData.note} />
        </Item>
      </Descriptions>
    )
  }

  return (
    <Popover
      title="Health Check"
      content={content}
      trigger="hover"
      onOpenChange={(open) => setHovered(open)}
    >
      <Tag color={healthCheckTagColor(healthCheck.status.name)}>
        {healthCheck.status.name}
      </Tag>
    </Popover>
  )
}

export default PiObjectiveHealthCheckTag

