import { useEffect, useState } from 'react'
import PageTitle from '../page-title'
import { getProgramIncrementsClient } from '@/src/services/clients'
import { Col, Divider, Row } from 'antd'
import ProgramIncrementCard from './program-increment-card'

const ActiveProgramIncrements = () => {
  const [activeProgramIncrements, setActiveProgramIncrements] = useState([])

  useEffect(() => {
    const loadActiveProgramIncrements = async () => {
      const programIncrementClient = await getProgramIncrementsClient()
      const programIncrementDtos = await programIncrementClient.getList()
      const activeProgramIncrements = programIncrementDtos.filter(
        (pi) => pi.state === 'Active'
      )
      setActiveProgramIncrements(activeProgramIncrements)
    }

    loadActiveProgramIncrements()
  }, [])

  const hasActiveProgramIncrements = activeProgramIncrements.length > 0

  function RenderContent() {
    if (hasActiveProgramIncrements) {
      return (
        <>
          <Row>
            <PageTitle title="Active Program Increments" />
          </Row>
          <Row>
            {activeProgramIncrements.map((pi) => (
              <Col
                key={pi.localId}
                xs={24}
                sm={12}
                md={8}
                lg={6}
                xl={6}
                xxl={4}
              >
                <ProgramIncrementCard programIncrement={pi} />
              </Col>
            ))}
          </Row>
          <Divider />
        </>
      )
    }
    return <></>
  }

  return <RenderContent />
}

export default ActiveProgramIncrements
