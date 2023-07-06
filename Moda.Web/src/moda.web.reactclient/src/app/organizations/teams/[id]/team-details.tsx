import { TeamDetailsDto } from '@/src/services/moda-api'
import { Col, Descriptions, Row } from 'antd'
import Link from 'next/link'
import ReactMarkdown from 'react-markdown'

const { Item } = Descriptions

const TeamDetails = (team: TeamDetailsDto) => {
  return (
    <>
      <Row>
        <Col xs={24} md={12}>
          <Descriptions>
            <Item label="Code">{team.code}</Item>
            <Item label="Type">{team.type}</Item>
            <Item label="Team of Teams">
              <Link
                href={`/organizations/team-of-teams/${team.teamOfTeams?.localId}`}
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
              <ReactMarkdown>{team.description}</ReactMarkdown>
            </Item>
          </Descriptions>
        </Col>
      </Row>
    </>
  )
}

export default TeamDetails
