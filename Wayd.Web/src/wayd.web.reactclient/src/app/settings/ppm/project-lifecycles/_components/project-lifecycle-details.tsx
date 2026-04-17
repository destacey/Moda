'use client'

import { ProjectLifecycleDetailsDto } from '@/src/services/moda-api'
import { Descriptions, Space } from 'antd'

const { Item } = Descriptions

interface ProjectLifecycleDetailsProps {
  lifecycle: ProjectLifecycleDetailsDto | undefined
}

const ProjectLifecycleDetails: React.FC<ProjectLifecycleDetailsProps> = ({
  lifecycle,
}: ProjectLifecycleDetailsProps) => {
  if (!lifecycle) return null

  return (
    <Space orientation="vertical">
      <Descriptions size="small">
        <Item label="State">{lifecycle.state?.name}</Item>
      </Descriptions>
      <Descriptions size="small">
        <Item label="Description">{lifecycle.description}</Item>
      </Descriptions>
    </Space>
  )
}

export default ProjectLifecycleDetails
