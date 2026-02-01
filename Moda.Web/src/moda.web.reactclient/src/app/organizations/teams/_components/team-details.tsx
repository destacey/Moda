'use client'

import LinksCard from '@/src/components/common/links/links-card'
import { MarkdownRenderer } from '@/src/components/common/markdown'
import ActiveTeamSprint from '@/src/components/common/planning/active-team-sprint'
import {
  Methodology,
  SizingMethod,
  TeamDetailsDto,
} from '@/src/services/moda-api'
import { Col, Descriptions, Divider, Flex, Row } from 'antd'
import dayjs from 'dayjs'
import Link from 'next/link'

const { Item } = Descriptions

const getSizingMethodDisplayName = (sizingMethod: SizingMethod): string => {
  return sizingMethod === SizingMethod.StoryPoints
    ? 'Story Points'
    : sizingMethod
}

interface TeamDetailsProps {
  team: TeamDetailsDto
}

const TeamDetails = ({ team }: TeamDetailsProps) => {
  if (!team) return null
  return (
    <Flex vertical>
      <Row gutter={[8, 8]}>
        <Col sm={24} md={12} lg={6}>
          <Descriptions column={1} size="small">
            <Item label="Code">{team.code}</Item>
            <Item label="Type">{team.type}</Item>
            <Item label="Parent Team">
              <Link
                href={`/organizations/team-of-teams/${team.teamOfTeams?.key}`}
              >
                {team.teamOfTeams?.name}
              </Link>
            </Item>
            <Item label="Active">
              {dayjs(team.activeDate).format('MMM D, YYYY')}
            </Item>
            {team.isActive === false && (
              <Item label="Inactive">
                {dayjs(team.inactiveDate).format('MMM D, YYYY')}
              </Item>
            )}
            <Item label="Methodology">{team.operatingModel?.methodology}</Item>
            <Item label="Sizing Method">
              {team.operatingModel &&
                getSizingMethodDisplayName(team.operatingModel.sizingMethod)}
            </Item>
          </Descriptions>
        </Col>

        <Col sm={24} md={12} lg={6}>
          <Descriptions layout="vertical" size="small">
            <Item label="Description">
              <MarkdownRenderer markdown={team?.description} />
            </Item>
          </Descriptions>
        </Col>

        {team.operatingModel?.methodology === Methodology.Scrum && (
          <Col sm={24} md={12} lg={12}>
            <ActiveTeamSprint teamId={team.id} />
          </Col>
        )}
      </Row>
      <Divider />
      <LinksCard objectId={team.id} />
    </Flex>
  )
}

export default TeamDetails
