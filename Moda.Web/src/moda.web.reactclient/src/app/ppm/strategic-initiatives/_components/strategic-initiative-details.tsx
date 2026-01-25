'use client'

import { ModaDateRange, ResponsiveFlex } from '@/src/components/common'
import LinksCard from '@/src/components/common/links/links-card'
import { MarkdownRenderer } from '@/src/components/common/markdown'
import { StrategicInitiativeDetailsDto } from '@/src/services/moda-api'
import { getSortedNames } from '@/src/utils'
import { Descriptions, Flex } from 'antd'
import Link from 'next/link'

const { Item } = Descriptions

export interface StrategicInitiativeDetailsProps {
  strategicInitiative: StrategicInitiativeDetailsDto
}

const StrategicInitiativeDetails: React.FC<StrategicInitiativeDetailsProps> = ({
  strategicInitiative,
}: StrategicInitiativeDetailsProps) => {
  if (!strategicInitiative) return null

  const sponsorNames =
    strategicInitiative?.strategicInitiativeSponsors.length > 0
      ? getSortedNames(strategicInitiative.strategicInitiativeSponsors)
      : 'No sponsors assigned'
  const ownerNames =
    strategicInitiative?.strategicInitiativeOwners.length > 0
      ? getSortedNames(strategicInitiative.strategicInitiativeOwners)
      : 'No owners assigned'

  return (
    <Flex vertical gap="middle">
      <ResponsiveFlex gap="middle" align="start">
        <Descriptions column={1} size="small">
          <Item label="Portfolio">
            <Link href={`/ppm/portfolios/${strategicInitiative.portfolio.key}`}>
              {strategicInitiative.portfolio.name}
            </Link>
          </Item>
          <Item label="Dates">
            <ModaDateRange
              dateRange={{
                start: strategicInitiative.start,
                end: strategicInitiative.end,
              }}
            />
          </Item>
          <Item label="Sponsors">{sponsorNames}</Item>
          <Item label="Owners">{ownerNames}</Item>
        </Descriptions>
        <Descriptions layout="vertical" size="small">
          <Item label="Description">
            <MarkdownRenderer markdown={strategicInitiative.description} />
          </Item>
        </Descriptions>
      </ResponsiveFlex>
      <LinksCard objectId={strategicInitiative.id} />
    </Flex>
  )
}

export default StrategicInitiativeDetails
