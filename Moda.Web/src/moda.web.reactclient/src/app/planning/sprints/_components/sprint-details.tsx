'use client'

import { IterationState } from '@/src/components/types'
import { SprintDetailsDto } from '@/src/services/moda-api'
import { Descriptions, Flex } from 'antd'
import Link from 'next/link'
import SprintMetrics from './sprint-metrics'
import IterationDates from '@/src/components/common/planning/iteration-dates'

const { Item: DescriptionItem } = Descriptions

export interface SprintDetailsProps {
  sprint: SprintDetailsDto
}

const SprintDetails: React.FC<SprintDetailsProps> = ({
  sprint,
}: SprintDetailsProps) => {
  if (!sprint) return null

  const sprintState = sprint.state.id as IterationState
  const showMetrics =
    sprintState === IterationState.Active ||
    sprintState === IterationState.Completed

  return (
    <Flex vertical gap={16}>
      <Descriptions column={4}>
        <DescriptionItem label="Team">
          <Link href={`/organizations/teams/${sprint.team?.key}`}>
            {sprint.team?.name}
          </Link>
        </DescriptionItem>
      </Descriptions>
      <IterationDates start={sprint.start} end={sprint.end} />
      {showMetrics && <SprintMetrics sprint={sprint} />}
    </Flex>
  )
}

export default SprintDetails
