import LinksCard from '@/src/app/components/common/links/links-card'
import MarkdownRenderer from '@/src/app/components/common/markdown-renderer'
import { TeamDetailsDto } from '@/src/services/moda-api'
import { Col, Descriptions, Row, Space } from 'antd'
import dayjs from 'dayjs'
import Link from 'next/link'

const { Item } = Descriptions

interface TeamDetailsProps {
  team: TeamDetailsDto
}

const TeamDetails = ({ team }: TeamDetailsProps) => {
  if (!team) return null
  return (
    <>
      <Space direction="vertical">
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
        <LinksCard objectId={team.id} />
      </Space>
    </>
  )
}

export default TeamDetails
