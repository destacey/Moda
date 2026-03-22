'use client'

import { Col, Divider, Row } from 'antd'
import ActivePlanningIntervals from '../components/common/planning/active-planning-intervals'
import MyAssignedRisks from '../components/common/planning/my-assigned-risks'
import MyProjectsCard from './ppm/dashboards/my-projects/_components/my-projects-card'
import { useDocumentTitle } from '../hooks/use-document-title'

const HomePage = () => {
  useDocumentTitle('Home')

  // TODO: have these load after the page is loaded
  return (
    <>
      <ActivePlanningIntervals />
      <Divider />
      <Row gutter={[16, 16]}>
        <Col xs={24} sm={24} md={12} lg={8}>
          <MyProjectsCard />
        </Col>
        <Col xs={24} sm={24} md={12} lg={8}>
          <MyAssignedRisks />
        </Col>
      </Row>
    </>
  )
}

export default HomePage
