import { useEffect, useState } from 'react'
import PageTitle from '../page-title'
import { Col, Divider, Row, Space } from 'antd'
import ProgramIncrementCard from './program-increment-card'
import dayjs from 'dayjs'
import { useGetProgramIncrements } from '@/src/services/queries/planning-queries'

const isWithinTwoWeeks = (date: Date) => {
  const twoWeeksFromNow = dayjs().add(14, 'day')
  return dayjs(date) <= twoWeeksFromNow
}

const ActiveProgramIncrements = () => {
  const { data: piData } = useGetProgramIncrements()
  const [activeProgramIncrements, setActiveProgramIncrements] = useState([])

  useEffect(() => {
    const activeProgramIncrements = piData
      ?.filter(
        (pi) =>
          pi.state === 'Active' ||
          (pi.state === 'Future' && isWithinTwoWeeks(pi.start)),
      )
      ?.sort((a, b) => {
        return dayjs(a.start).unix() - dayjs(b.start).unix()
      })
    setActiveProgramIncrements(activeProgramIncrements)
  }, [piData])

  if (!activeProgramIncrements || activeProgramIncrements?.length == 0) return

  return (
    <>
      <Row>
        <PageTitle title="Program Increments" />
      </Row>
      <Row>
        <Space>
          {activeProgramIncrements?.map((pi) => (
            <Col key={pi.key}>
              <ProgramIncrementCard programIncrement={pi} />
            </Col>
          ))}
        </Space>
      </Row>
      <Divider />
    </>
  )
}

export default ActiveProgramIncrements
