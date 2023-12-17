import { useEffect, useState } from 'react'
import PageTitle from '../page-title'
import { Col, Divider, Row, Space } from 'antd'
import PlanningIntervalCard from './planning-interval-card'
import dayjs from 'dayjs'
import { useGetPlanningIntervals } from '@/src/services/queries/planning-queries'

const isWithinTwoWeeks = (date: Date) => {
  const twoWeeksFromNow = dayjs().add(14, 'day')
  return dayjs(date) <= twoWeeksFromNow
}

const ActivePlanningIntervals = () => {
  const { data: piData } = useGetPlanningIntervals()
  const [activePlanningIntervals, setActivePlanningIntervals] = useState([])

  useEffect(() => {
    const activePlanningIntervals = piData
      ?.filter(
        (pi) =>
          pi.state === 'Active' ||
          (pi.state === 'Future' && isWithinTwoWeeks(pi.start)),
      )
      ?.sort((a, b) => {
        return dayjs(a.start).unix() - dayjs(b.start).unix()
      })
    setActivePlanningIntervals(activePlanningIntervals)
  }, [piData])

  if (!activePlanningIntervals || activePlanningIntervals?.length == 0) return

  return (
    <>
      <Row>
        <PageTitle title="Planning Intervals" />
      </Row>
      <Row>
        <Space>
          {activePlanningIntervals?.map((pi) => (
            <Col key={pi.key}>
              <PlanningIntervalCard planningInterval={pi} />
            </Col>
          ))}
        </Space>
      </Row>
      <Divider />
    </>
  )
}

export default ActivePlanningIntervals
