'use client'

import { Col, Row } from 'antd'
import ActiveProgramIncrements from './components/common/planning/active-program-increments'
import MyAssignedRisks from './components/common/planning/my-assigned-risks'
import { useDocumentTitle } from './hooks/use-document-title'

const HomePage = () => {
  useDocumentTitle('Home')

  return (
    <>
      <ActiveProgramIncrements />
      <Row>
        <Col>
          <MyAssignedRisks />
        </Col>
      </Row>
    </>
  )
}

export default HomePage
