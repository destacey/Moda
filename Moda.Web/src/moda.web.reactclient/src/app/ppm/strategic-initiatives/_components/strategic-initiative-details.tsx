'use client'

import { ModaDateRange, ResponsiveFlex } from '@/src/components/common'
import { MarkdownRenderer } from '@/src/components/common/markdown'
import { StrategicInitiativeDetailsDto } from '@/src/services/moda-api'
import { getSortedNames } from '@/src/utils'
import { Descriptions, List } from 'antd'
import Link from 'next/link'
import { StrategicInitiativeKpiListCard } from '.'

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
    <>
      <ResponsiveFlex gap="middle" align="start">
        <Descriptions column={1} size="small">
          <Item label="Status">{strategicInitiative.status.name}</Item>
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
      <StrategicInitiativeKpiListCard
        strategicInitiativeId={strategicInitiative.id}
      />
    </>
  )
}

export default StrategicInitiativeDetails
