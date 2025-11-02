'use client'

import LinksCard from '@/src/components/common/links/links-card'
import { SprintDetailsDto } from '@/src/services/moda-api'
import { Descriptions, Flex } from 'antd'
import dayjs from 'dayjs'
import Link from 'next/link'

const { Item: DescriptionItem } = Descriptions

const DATE_TIME_FORMAT = 'MMM D, YYYY h:mm A'

export interface SprintDetailsProps {
  sprint: SprintDetailsDto
}

const SprintDetails: React.FC<SprintDetailsProps> = ({
  sprint,
}: SprintDetailsProps) => {
  if (!sprint) return null

  return (
    <Flex vertical gap={16}>
      <Descriptions column={1}>
        <DescriptionItem label="Team">
          <Link href={`/organizations/teams/${sprint.team?.key}`}>
            {sprint.team?.name}
          </Link>
        </DescriptionItem>
        <DescriptionItem label="State">{sprint.state.name}</DescriptionItem>
        <DescriptionItem label="Start">
          {dayjs(sprint.start).format(DATE_TIME_FORMAT)}
        </DescriptionItem>
        <DescriptionItem label="End">
          {dayjs(sprint.end).format(DATE_TIME_FORMAT)}
        </DescriptionItem>
      </Descriptions>
      <LinksCard objectId={sprint.id} />
    </Flex>
  )
}

export default SprintDetails
