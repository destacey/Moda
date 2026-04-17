'use client'

import { LifecycleStatusTag, WaydDateRange } from '@/src/components/common'
import { LinkOutlined } from '@ant-design/icons'
import { ProjectDetailsDto } from '@/src/services/wayd-api'
import { getSortedNames } from '@/src/utils'
import { Button, Flex, Typography } from 'antd'
import { WaydTooltip } from '@/src/components/common'
import Link from 'next/link'
import { FC } from 'react'
import styles from '../my-projects-dashboard.module.css'

const { Title, Text } = Typography

export interface ProjectDetailHeaderProps {
  project: ProjectDetailsDto
}

const ProjectDetailHeader: FC<ProjectDetailHeaderProps> = ({ project }) => {
  const ownerNames = getSortedNames(project.projectOwners)
  const pmNames = getSortedNames(project.projectManagers)

  return (
    <div className={styles.detailHeader}>
      <Flex vertical gap={8}>
        <Flex align="center" gap={4}>
          <Title level={4} style={{ margin: 0 }}>
            {project.name}
          </Title>
          <WaydTooltip title="Open project details">
            <Link href={`/ppm/projects/${project.key}`}>
              <Button
                type="text"
                size="small"
                icon={<LinkOutlined style={{ fontSize: 11 }} />}
              />
            </Link>
          </WaydTooltip>
          <LifecycleStatusTag status={project.status} />
        </Flex>
        <Flex gap={12} align="center" wrap>
          {project.program && (
            <Text type="secondary" style={{ fontSize: 12 }}>
              Program:{' '}
              <Link href={`/ppm/programs/${project.program.key}`}>
                {project.program.name}
              </Link>
            </Text>
          )}
          {ownerNames && (
            <Text type="secondary" style={{ fontSize: 12 }}>
              Owner: {ownerNames}
            </Text>
          )}
          {pmNames && (
            <Text type="secondary" style={{ fontSize: 12 }}>
              PM: {pmNames}
            </Text>
          )}
          {(project.start || project.end) && (
            <Text type="secondary" style={{ fontSize: 12 }}>
              <WaydDateRange
                dateRange={{ start: project.start, end: project.end }}
              />
            </Text>
          )}
          <Text type="secondary" style={{ fontSize: 12 }}>
            Lifecycle:{' '}
            {project.projectLifecycle?.name ?? 'No lifecycle assigned'}
          </Text>
        </Flex>
      </Flex>
    </div>
  )
}

export default ProjectDetailHeader

