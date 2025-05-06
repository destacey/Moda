import { useMemo } from 'react'
import { Col, Row, Space, Typography } from 'antd'
import PlanningIntervalCard from './planning-interval-card'
import dayjs from 'dayjs'
import { useGetPlanningIntervalsQuery } from '@/src/store/features/planning/planning-interval-api'

const { Title } = Typography

const isWithinTwoWeeks = (date: Date) => {
  const twoWeeksFromNow = dayjs().add(14, 'day')
  return (
    dayjs(date).isBefore(twoWeeksFromNow) || dayjs(date).isSame(twoWeeksFromNow)
  )
}

const ActivePlanningIntervals = () => {
  const { data: piData } = useGetPlanningIntervalsQuery()

  const activePlanningIntervals = useMemo(() => {
    return (
      piData
        ?.filter(
          (pi) =>
            pi.state === 'Active' ||
            (pi.state === 'Future' && isWithinTwoWeeks(pi.start)),
        )
        ?.sort((a, b) => dayjs(a.start).unix() - dayjs(b.start).unix()) || []
    )
  }, [piData])

  if (activePlanningIntervals.length === 0) {
    return <div>No active planning intervals found.</div>
  }

  return (
    <>
      <Space direction="vertical">
        <Title level={2} style={{ margin: '0px', fontWeight: '400' }}>
          Planning Intervals
        </Title>
        <Row gutter={[16, 16]}>
          {activePlanningIntervals.map((pi) => (
            <Col key={pi.key}>
              <PlanningIntervalCard planningInterval={pi} />
            </Col>
          ))}
        </Row>
      </Space>
    </>
  )
}

export default ActivePlanningIntervals
