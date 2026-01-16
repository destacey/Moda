'use client'

import { ModaDateRange, ResponsiveFlex } from '@/src/components/common'
import LinksCard from '@/src/components/common/links/links-card'
import { MarkdownRenderer } from '@/src/components/common/markdown'
import { ProgramDetailsDto } from '@/src/services/moda-api'
import { getSortedNames } from '@/src/utils'
import { Descriptions, Flex } from 'antd'
import Link from 'next/link'
import { FC } from 'react'

const { Item } = Descriptions

export interface ProgramDetailsProps {
  program: ProgramDetailsDto
}

const ProgramDetails: FC<ProgramDetailsProps> = ({
  program,
}: ProgramDetailsProps) => {
  if (!program) return null

  const sponsorNames =
    program?.programSponsors.length > 0
      ? getSortedNames(program.programSponsors)
      : 'No sponsors assigned'
  const ownerNames =
    program?.programOwners.length > 0
      ? getSortedNames(program.programOwners)
      : 'No owners assigned'
  const managerNames =
    program?.programManagers.length > 0
      ? getSortedNames(program.programManagers)
      : 'No managers assigned'

  const strategicThemes =
    program?.strategicThemes.length > 0
      ? getSortedNames(program.strategicThemes)
      : null

  return (
    <Flex vertical gap="middle">
      <ResponsiveFlex gap="middle" align="start">
        <Descriptions column={1} size="small">
          <Item label="Portfolio">
            <Link href={`/ppm/portfolios/${program.portfolio.key}`}>
              {program.portfolio.name}
            </Link>
          </Item>
          <Item label="Dates">
            <ModaDateRange
              dateRange={{ start: program.start, end: program.end }}
            />
          </Item>
          <Item label="Sponsors">{sponsorNames}</Item>
          <Item label="Owners">{ownerNames}</Item>
          <Item label="Managers">{managerNames}</Item>
          <Item label="Strategic Themes">{strategicThemes}</Item>
        </Descriptions>
        <Descriptions layout="vertical" size="small">
          <Item label="Description">
            <MarkdownRenderer markdown={program.description} />
          </Item>
        </Descriptions>
      </ResponsiveFlex>
      <LinksCard objectId={program.id} />
    </Flex>
  )
}

export default ProgramDetails
