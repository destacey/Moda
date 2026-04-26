'use client'

import { CycleTimeMetric, MetricCard } from '@/src/components/common/metrics'
import WaydTooltip from '@/src/components/common/wayd-tooltip'
import {
  AimOutlined,
  CheckCircleOutlined,
  PlusCircleOutlined,
} from '@ant-design/icons'
import TimelineProgress from '@/src/components/common/planning/timeline-progress'
import { IterationState } from '@/src/components/types'
import { PlanningIntervalDetailsDto } from '@/src/services/wayd-api'
import {
  useGetPlanningIntervalMetricsQuery,
  useGetPlanningIntervalObjectivesQuery,
  useGetPlanningIntervalPredictabilityQuery,
  useGetPlanningIntervalTeamsQuery,
} from '@/src/store/features/planning/planning-interval-api'
import { Card, Col, Flex, Row } from 'antd'
import {
  ObjectiveHealthChart,
  ObjectiveStatusChart,
  TeamPredictabilityRadarChart,
} from '../../_components'

interface PlanningIntervalAtAGlanceProps {
  planningInterval: PlanningIntervalDetailsDto
}

const PlanningIntervalAtAGlance = ({
  planningInterval,
}: PlanningIntervalAtAGlanceProps) => {
  const { data: piPredictabilityData, isLoading: isLoadingPiPredictability } =
    useGetPlanningIntervalPredictabilityQuery(planningInterval.key, {
      skip: !planningInterval.key,
    })

  const { data: teamsData } = useGetPlanningIntervalTeamsQuery(
    planningInterval.key,
    { skip: !planningInterval.key },
  )

  const { data: piMetricsData } = useGetPlanningIntervalMetricsQuery(
    planningInterval.key,
    {
      skip:
        !planningInterval.key ||
        planningInterval.state.id === IterationState.Future,
    },
  )

  const { data: objectivesData } = useGetPlanningIntervalObjectivesQuery(
    {
      planningIntervalKey: planningInterval.key,
      teamId: null,
    },
    {
      skip:
        !planningInterval.key ||
        planningInterval.state.id === IterationState.Future,
    },
  )

  const hasObjectives = !!objectivesData && objectivesData.length > 0

  const objectiveCounts = (() => {
    if (!objectivesData) return { regular: 0, stretch: 0, completed: 0 }
    return objectivesData.reduce(
      (acc, o) => {
        if (o.isStretch) acc.stretch += 1
        else acc.regular += 1
        if (o.status.name === 'Completed') acc.completed += 1
        return acc
      },
      { regular: 0, stretch: 0, completed: 0 },
    )
  })()

  return (
    <Card size="small" title="At a Glance">
      <Flex vertical gap="middle">
        {/*
          Breakpoints chosen so cards/charts always have enough room for their content:
          - Top row: three small cards. Stack on phones, 2-up on small/medium, 3-up from lg.
          - Charts: 3 of them, so go straight from stacked (xs) to 3-up at lg.
            A 2-up step would always leave the 3rd chart alone on its own row.
        */}
        <Row gutter={[16, 16]} align="stretch">
          <Col xs={24} sm={12} md={12} lg={6}>
            <TimelineProgress
              start={new Date(planningInterval.start)}
              end={new Date(planningInterval.end)}
              style={{ height: '100%', width: '100%', minWidth: 0 }}
            />
          </Col>
          <Col xs={12} sm={6} md={6} lg={6}>
            <MetricCard
              title="Teams"
              value={teamsData?.filter((t) => t.type === 'Team').length ?? 0}
              tooltip="Number of teams participating in this planning interval. Excludes teams of teams."
            />
          </Col>
          {hasObjectives && (
            <Col xs={12} sm={6} md={6} lg={6}>
              <MetricCard
                title="PI Predictability"
                value={planningInterval.predictability ?? 0}
                precision={0}
                suffix="%"
                tooltip="The percentage of completed objectives compared to the number of non-stretched objectives in this planning interval."
                tooltipTarget="title"
                secondaryValue={
                  <Flex gap={12} style={{ fontSize: 12 }}>
                    <WaydTooltip title="Completed">
                      <span>
                        <CheckCircleOutlined
                          style={{ marginRight: 4 }}
                          aria-label="Completed"
                        />
                        {objectiveCounts.completed}
                      </span>
                    </WaydTooltip>
                    <WaydTooltip title="Regular (non-stretch)">
                      <span>
                        <AimOutlined
                          style={{ marginRight: 4 }}
                          aria-label="Regular"
                        />
                        {objectiveCounts.regular}
                      </span>
                    </WaydTooltip>
                    <WaydTooltip title="Stretch">
                      <span>
                        <PlusCircleOutlined
                          style={{ marginRight: 4 }}
                          aria-label="Stretch"
                        />
                        {objectiveCounts.stretch}
                      </span>
                    </WaydTooltip>
                  </Flex>
                }
              />
            </Col>
          )}
          {piMetricsData?.cycleTime &&
            piMetricsData.cycleTime.workItemsCount > 0 && (
              <Col xs={12} sm={6} md={6} lg={6}>
                <CycleTimeMetric
                  value={piMetricsData.cycleTime.averageCycleTimeDays ?? 0}
                  tooltip="Average cycle time of completed (Done) work items across all team sprints in every iteration of this planning interval."
                />
              </Col>
            )}
        </Row>
        {hasObjectives && (
          <Row gutter={[16, 16]} align="stretch">
            <Col xs={24} md={12} lg={8}>
              <TeamPredictabilityRadarChart
                teamPredictabilities={
                  piPredictabilityData?.teamPredictabilities
                }
                isLoading={isLoadingPiPredictability}
              />
            </Col>
            <Col xs={24} md={12} lg={8}>
              <ObjectiveStatusChart objectivesData={objectivesData ?? []} />
            </Col>
            <Col xs={24} md={12} lg={8}>
              <ObjectiveHealthChart objectivesData={objectivesData ?? []} />
            </Col>
          </Row>
        )}
      </Flex>
    </Card>
  )
}

export default PlanningIntervalAtAGlance
