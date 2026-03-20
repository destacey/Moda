import PhaseTimeline from '../../_components/phase-timeline'
import { ProjectDetailsDto } from '@/src/services/moda-api'
import { Col, Flex, Row, Typography } from 'antd'
import { ProjectDetails } from '.'
import ProjectTaskMetrics from './project-task-metrics'
import { FC } from 'react'
import { ModaEmpty } from '@/src/components/common'

const { Text } = Typography

export interface ProjectDetailsTabProps {
  project: ProjectDetailsDto
}

const ProjectDetailsTab: FC<ProjectDetailsTabProps> = ({ project }) => {
  if (!project) return null

  return (
    <Row gutter={[16, 16]}>
      <Col xs={24} md={9} xxl={6}>
        <ProjectDetails project={project} />
      </Col>
      <Col xs={24} md={15} xxl={18}>
        {project.projectLifecycle && (
          <Flex vertical gap="large">
            <Flex align="center" gap={8}>
              <Text strong>Phases</Text>
            </Flex>
            <PhaseTimeline phases={project.phases} />
            <Flex align="center" gap={8}>
              <Text strong>Task Summary</Text>
            </Flex>
            <ProjectTaskMetrics projectKey={project.key} />
          </Flex>
        )}

        {!project.projectLifecycle && (
          <ModaEmpty message="No lifecycle defined for this project." />
        )}
      </Col>
    </Row>
  )
}

export default ProjectDetailsTab

