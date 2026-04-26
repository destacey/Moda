'use client'

import { CycleTimeMetric, MetricCard } from '@/src/components/common/metrics'
import WaydTooltip from '@/src/components/common/wayd-tooltip'
import { useGetPlanningIntervalMetricsQuery } from '@/src/store/features/planning/planning-interval-api'
import {
  AimOutlined,
  CheckCircleOutlined,
  PlusCircleOutlined,
} from '@ant-design/icons'
import { Card, Col, Flex, Row, Skeleton, Typography } from 'antd'
import Link from 'next/link'

const { Text } = Typography

interface PlanningIntervalTeamCardsProps {
  piKey: number
}

const PlanningIntervalTeamCards = ({
  piKey,
}: PlanningIntervalTeamCardsProps) => {
  const { data, isLoading } = useGetPlanningIntervalMetricsQuery(piKey, {
    skip: !piKey,
  })

  if (isLoading) {
    return (
      <Card size="small" title="Teams">
        <Skeleton active paragraph={{ rows: 2 }} />
      </Card>
    )
  }

  const teamMetrics = data?.teamMetrics ?? []
  if (teamMetrics.length === 0) return null

  return (
    <Card
      size="small"
      title="Teams"
      extra={
        <Text type="secondary">
          Click a team card to open its plan review →
        </Text>
      }
    >
      <Row gutter={[16, 16]}>
        {teamMetrics.map((tm) => (
          <Col xs={24} md={12} lg={8} key={tm.team.id}>
            <Link
              href={`/planning/planning-intervals/${piKey}/plan-review#${tm.teamCode.toLowerCase()}`}
              style={{ display: 'block', height: '100%', color: 'inherit' }}
            >
              <Card size="small" hoverable style={{ height: '100%' }}>
                <Flex vertical gap="small">
                  <Text strong>{tm.team.name}</Text>
                  <Row gutter={[8, 8]}>
                    <Col span={12}>
                      <MetricCard
                        embedded
                        title="Predictability"
                        value={tm.predictability ?? 0}
                        precision={0}
                        suffix="%"
                        tooltip="The team's predictability for this planning interval — completed objectives over committed (non-stretch) objectives."
                        tooltipTarget="title"
                        secondaryValue={
                          <Flex gap={12} style={{ fontSize: 12 }}>
                            <WaydTooltip title="Completed">
                              <span>
                                <CheckCircleOutlined
                                  style={{ marginRight: 4 }}
                                  aria-label="Completed"
                                />
                                {tm.completedObjectivesCount}
                              </span>
                            </WaydTooltip>
                            <WaydTooltip title="Regular (non-stretch)">
                              <span>
                                <AimOutlined
                                  style={{ marginRight: 4 }}
                                  aria-label="Regular"
                                />
                                {tm.regularObjectivesCount}
                              </span>
                            </WaydTooltip>
                            <WaydTooltip title="Stretch">
                              <span>
                                <PlusCircleOutlined
                                  style={{ marginRight: 4 }}
                                  aria-label="Stretch"
                                />
                                {tm.stretchObjectivesCount}
                              </span>
                            </WaydTooltip>
                          </Flex>
                        }
                      />
                    </Col>
                    <Col span={12}>
                      <CycleTimeMetric
                        embedded
                        value={tm.cycleTime?.averageCycleTimeDays ?? 0}
                        tooltip="Average cycle time across this team's sprints in this planning interval."
                      />
                    </Col>
                  </Row>
                </Flex>
              </Card>
            </Link>
          </Col>
        ))}
      </Row>
    </Card>
  )
}

export default PlanningIntervalTeamCards

