'use client'

import LinksCard from '@/src/components/common/links/links-card'
import { MarkdownRenderer } from '@/src/components/common/markdown'
import { TeamOfTeamsDetailsDto } from '@/src/services/wayd-api'
import { Col, Descriptions, Divider, Flex, Row, Space } from 'antd'
import dayjs from 'dayjs'
import Link from 'next/link'
import { useGetTeamMembersQuery } from '@/src/store/features/organization/team-members-api'
import TeamMemberCard from '../../teams/_components/team-member-card'

const { Item } = Descriptions

interface TeamOfTeamsDetailsProps {
  team: TeamOfTeamsDetailsDto
}

const TeamOfTeamsDetails = ({ team }: TeamOfTeamsDetailsProps) => {
  const { data: members } = useGetTeamMembersQuery(
    { teamId: team?.id ?? '' },
    { skip: !team?.id },
  )

  if (!team) return null
  return (
    <Flex vertical>
      <Space vertical>
        <Row>
          <Col xs={24} md={12}>
            <Descriptions>
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
            </Descriptions>
          </Col>
          <Col xs={24} md={12}>
            <Descriptions layout="vertical">
              <Item label="Description">
                <MarkdownRenderer markdown={team?.description} />
              </Item>
            </Descriptions>
          </Col>
        </Row>
      </Space>

      {members && members.length > 0 && (
        <>
          <Divider titlePlacement="start">Members</Divider>
          <Row gutter={[8, 8]}>
            {members.map((member) => (
              <Col key={member.employee.id} xs={24} sm={12} md={8} lg={6}>
                <TeamMemberCard member={member} />
              </Col>
            ))}
          </Row>
        </>
      )}

      <Divider />
      <LinksCard objectId={team.id} />
    </Flex>
  )
}

export default TeamOfTeamsDetails

