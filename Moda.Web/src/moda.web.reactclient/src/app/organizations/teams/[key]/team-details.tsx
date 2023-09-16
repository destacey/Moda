import LinksCard from '@/src/app/components/common/links/links-card'
import { TeamDetailsDto } from '@/src/services/moda-api'
import { Col, Descriptions, Row, Space } from 'antd'
import Link from 'next/link'
import ReactMarkdown from 'react-markdown'

const { Item } = Descriptions

interface TeamDetailsProps {
  team: TeamDetailsDto
}

const TeamDetails = ({ team }: TeamDetailsProps) => {
  if (!team) return null
  return (
    <>
      <Row>
        <Col xs={24} md={12}>
          <Descriptions>
            <Item label="Code">{team.code}</Item>
            <Item label="Type">{team.type}</Item>
            <Item label="Team of Teams">
              <Link
                href={`/organizations/team-of-teams/${team.teamOfTeams?.key}`}
              >
                {team.teamOfTeams?.name}
              </Link>
            </Item>
            <Item label="Is Active?">{team.isActive?.toString()}</Item>
          </Descriptions>
        </Col>
        <Col xs={24} md={12}>
          <Descriptions layout="vertical">
            <Item label="Description">
              <Space direction="vertical">
                <ReactMarkdown>{team.description}</ReactMarkdown>
              </Space>
            </Item>
          </Descriptions>
        </Col>
      </Row>
      <LinksCard objectId={team.id} />
    </>
  )
}

export default TeamDetails
