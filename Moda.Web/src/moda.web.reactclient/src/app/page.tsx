'use client'

import { Col, Divider, Row } from 'antd'
import ActivePlanningIntervals from './components/common/planning/active-planning-intervals'
import MyAssignedRisks from './components/common/planning/my-assigned-risks'
import { useDocumentTitle } from './hooks/use-document-title'

const HomePage = () => {
  useDocumentTitle('Home')

  // TODO: have these load after the page is loaded
  return (
    <>
      <ActivePlanningIntervals />
      <Divider />
      <Row>
        <Col>
          <MyAssignedRisks />
        </Col>
      </Row>
    </>
  )
}

export default HomePage
