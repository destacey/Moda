'use client'

import { ProjectHealthCheckDto } from '@/src/services/wayd-api'
import { useState } from 'react'
import { useGetProjectHealthCheckQuery } from '@/src/store/features/ppm/project-health-checks-api'
import HealthCheckTag from '../../../../components/common/health-check/health-check-tag'

export interface ProjectHealthCheckTagProps {
  healthCheck?: ProjectHealthCheckDto
  projectId?: string
}

const ProjectHealthCheckTag = ({
  healthCheck,
  projectId,
}: ProjectHealthCheckTagProps) => {
  const [hovered, setHovered] = useState(false)
  const canFetchDetails = !!projectId && !!healthCheck?.id

  const { data: healthCheckData, isLoading } = useGetProjectHealthCheckQuery(
    {
      projectId: projectId ?? '',
      healthCheckId: healthCheck?.id ?? '',
    },
    { skip: !hovered || !canFetchDetails },
  )

  return (
    <HealthCheckTag
      healthCheck={healthCheck}
      details={canFetchDetails ? healthCheckData : null}
      isLoading={canFetchDetails ? isLoading : false}
      onOpenChange={setHovered}
    />
  )
}

export default ProjectHealthCheckTag
