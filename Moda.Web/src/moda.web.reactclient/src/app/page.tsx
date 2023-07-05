'use client'

import { Col, Row } from 'antd'
import ActiveProgramIncrements from './components/common/planning/active-program-increments'
import MyAssignedRisks from './components/common/planning/my-assigned-risks'
import { useDocumentTitle } from './hooks/use-document-title'
import useBreadcrumbs from './components/contexts/breadcrumbs/use-breadcrumbs'
import { useEffect } from 'react'

export const metadata = {
  title: {
    absolute: 'Home',
  },
}

const HomePage = () => {
  useDocumentTitle('Home')
  const { setBreadcrumbs } = useBreadcrumbs()

  useEffect(() => {
    setBreadcrumbs([])
  }, [setBreadcrumbs])

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
