import { useEffect, useState } from 'react'
import PageTitle from '../page-title'
import { getProgramIncrementsClient } from '@/src/services/clients'
import { Col, Divider, Row, Space } from 'antd'
import ProgramIncrementCard from './program-increment-card'
import dayjs from 'dayjs'

const isWithinTwoWeeks = (date: Date) => {
  const twoWeeksFromNow = dayjs().add(14, 'day')
  return dayjs(date) <= twoWeeksFromNow
}

const ActiveProgramIncrements = () => {
  const [activeProgramIncrements, setActiveProgramIncrements] = useState([])

  useEffect(() => {
    const loadActiveProgramIncrements = async () => {
      const programIncrementClient = await getProgramIncrementsClient()
      const programIncrementDtos = await programIncrementClient.getList()
      const activeProgramIncrements = programIncrementDtos.filter(
        (pi) =>
          pi.state === 'Active' ||
          (pi.state === 'Future' && isWithinTwoWeeks(pi.start))
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
            <PageTitle title="Program Increments" />
          </Row>
          <Row>
            <Space>
              {activeProgramIncrements.map((pi) => (
                <Col key={pi.localId}>
                  <ProgramIncrementCard programIncrement={pi} />
                </Col>
              ))}
            </Space>
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
