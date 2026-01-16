'use client'

import { ResponsiveFlex } from '@/src/components/common'
import LinksCard from '@/src/components/common/links/links-card'
import { MarkdownRenderer } from '@/src/components/common/markdown'
import { ProjectPortfolioDetailsDto } from '@/src/services/moda-api'
import { getSortedNames } from '@/src/utils'
import { Descriptions, Flex } from 'antd'

const { Item } = Descriptions

export interface PortfolioDetailsProps {
  portfolio: ProjectPortfolioDetailsDto
}

const PortfolioDetails: React.FC<PortfolioDetailsProps> = ({
  portfolio,
}: PortfolioDetailsProps) => {
  if (!portfolio) return null

  const sponsorNames =
    portfolio?.portfolioSponsors.length > 0
      ? getSortedNames(portfolio.portfolioSponsors)
      : 'No sponsors assigned'
  const ownerNames =
    portfolio?.portfolioOwners.length > 0
      ? getSortedNames(portfolio.portfolioOwners)
      : 'No owners assigned'
  const managerNames =
    portfolio?.portfolioManagers.length > 0
      ? getSortedNames(portfolio.portfolioManagers)
      : 'No managers assigned'

  return (
    <Flex vertical gap="middle">
      <ResponsiveFlex gap="middle" align="start">
        <Descriptions column={1} size="small">
          <Item label="Sponsors">{sponsorNames}</Item>
          <Item label="Owners">{ownerNames}</Item>
          <Item label="Managers">{managerNames}</Item>
        </Descriptions>
        <Descriptions layout="vertical" size="small">
          <Item label="Description">
            <MarkdownRenderer markdown={portfolio.description} />
          </Item>
        </Descriptions>
      </ResponsiveFlex>
      <LinksCard objectId={portfolio.id} />
    </Flex>
  )
}

export default PortfolioDetails
