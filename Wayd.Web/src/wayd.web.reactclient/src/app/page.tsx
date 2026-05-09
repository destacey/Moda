'use client'

import { Col, Divider, Row } from 'antd'
import ActivePlanningIntervals from '../components/common/planning/active-planning-intervals'
import MyAssignedRisks from '../components/common/planning/my-assigned-risks'
import MyTeamSprints from '../components/common/planning/my-team-sprints'
import MyProjectsCard from './ppm/dashboards/my-projects/_components/my-projects-card'
import { useDocumentTitle } from '../hooks/use-document-title'

const HomePage = () => {
  useDocumentTitle('Home')

  // TODO: have these load after the page is loaded
  return (
    <Row gutter={[16, 16]}>
      <Col xs={24} lg={16}>
        <ActivePlanningIntervals />
        <Divider />
        <Row gutter={[16, 16]}>
          <Col xs={24} md={12}>
            <MyProjectsCard />
          </Col>
          <Col xs={24} md={12}>
            <MyAssignedRisks />
          </Col>
        </Row>
      </Col>
      <Col xs={24} lg={8}>
        <MyTeamSprints />
      </Col>
    </Row>
  )
}

export default HomePage
