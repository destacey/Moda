'use client'

import LinksCard from '@/src/components/common/links/links-card'
import { PlanningIntervalDetailsDto } from '@/src/services/moda-api'
import {
  Card,
  Col,
  Descriptions,
  DescriptionsProps,
  Divider,
  Row,
  Space,
  Statistic,
} from 'antd'
import dayjs from 'dayjs'
import { useMemo } from 'react'
import {
  DaysCountdownMetric,
  MetricCard,
} from '@/src/components/common/metrics'
import {
  useGetPlanningIntervalObjectivesQuery,
  useGetPlanningIntervalPredictabilityQuery,
} from '@/src/store/features/planning/planning-interval-api'
import {
  ObjectiveHealthChart,
  ObjectiveHealthChartDataItem,
  ObjectiveStatusChart,
  ObjectiveStatusChartDataItem,
  PlanningIntervalIterationsList,
  TeamPredictabilityRadarChart,
} from '.'
import { MarkdownRenderer } from '@/src/components/common/markdown'
import { IterationState } from '@/src/components/types'

const { Item } = Descriptions

interface PlanningIntervalDetailsProps {
  planningInterval: PlanningIntervalDetailsDto
}

const PlanningIntervalDetails = ({
  planningInterval,
}: PlanningIntervalDetailsProps) => {
  const { data: piPredictabilityData, isLoading: isLoadingPiPredictability } =
    useGetPlanningIntervalPredictabilityQuery(planningInterval?.key, {
      skip: !planningInterval?.key,
    })

  const {
    data: objectivesData,
    isLoading: isLoadingObjectives,
    refetch: refectchObjectives,
  } = useGetPlanningIntervalObjectivesQuery(
    {
      planningIntervalKey: planningInterval?.key,
      teamId: null,
    },
    {
      skip:
        !planningInterval?.key ||
        planningInterval?.state.id === IterationState.Future,
    },
  )

  const detailsItems: DescriptionsProps['items'] = [
    {
      key: 'start',
      label: 'Start',
      children: dayjs(planningInterval.start).format('MMM D, YYYY'),
    },
    {
      key: 'end',
      label: 'End',
      children: dayjs(planningInterval.end).format('MMM D, YYYY'),
    },
    {
      key: 'state.name',
      label: 'State',
      children: planningInterval.state.name,
    },
    {
      key: 'objectivesLocked',
      label: 'Objectives Locked?',
      children: planningInterval.objectivesLocked ? 'Yes' : 'No',
    },
  ]

  const { objectiveStatusData, objectiveHealthData } = useMemo(() => {
    if (!objectivesData)
      return { objectiveStatusData: [], objectiveHealthData: [] }
    const objectives = objectivesData.map((o) => ({
      status: o.status.name,
      health:
        o.status.name === 'Completed'
          ? 'Healthy'
          : o.status.name === 'Canceled' || o.status.name === 'Missed'
            ? 'Unhealthy'
            : (o.healthCheck?.status.name ?? 'Unknown'),
    }))

    // group by status with count
    const statusData = objectives.reduce(
      (acc, obj) => {
        acc[obj.status] = acc[obj.status] ? acc[obj.status] + 1 : 1
        return acc
      },
      {} as Record<string, number>,
    )

    const objectiveStatusData: ObjectiveStatusChartDataItem[] = Object.entries(
      statusData,
    ).map(([status, count]) => ({ type: status, count }))

    // group by status with count
    const healthData = objectives.reduce(
      (acc, obj) => {
        acc[obj.health] = acc[obj.health] ? acc[obj.health] + 1 : 1
        return acc
      },
      {} as Record<string, number>,
    )

    const objectiveHealthData: ObjectiveHealthChartDataItem[] = Object.entries(
      healthData,
    ).map(([health, count]) => ({ type: health, count }))

    return { objectiveStatusData, objectiveHealthData }
  }, [objectivesData])

  if (!planningInterval) return null

  return (
    <>
      <Row>
        <Col xs={24} sm={24} md={11} lg={13} xl={16}>
          <Descriptions
            size="small"
            column={{ xs: 1, sm: 1, md: 1, lg: 3, xl: 4, xxl: 5 }}
            items={detailsItems}
          />
          {planningInterval.description && (
            <Descriptions layout="vertical" size="small">
              <Item label="Description">
                <MarkdownRenderer markdown={planningInterval.description} />
              </Item>
            </Descriptions>
          )}
        </Col>
        <Col xs={24} sm={24} md={13} lg={11} xl={8}>
          <PlanningIntervalIterationsList
            planningIntervalKey={planningInterval?.key}
          />
        </Col>
      </Row>
      <Divider />
      <Space align="start" wrap>
        <DaysCountdownMetric
          state={planningInterval.state.id as IterationState}
          startDate={new Date(planningInterval.start)}
          endDate={new Date(planningInterval.end)}
        />
        {objectivesData && objectivesData.length > 0 && (
          <>
            <MetricCard
              title="PI Predictability"
              value={planningInterval.predictability ?? 0}
              precision={0}
              suffix="%"
              tooltip="The percentage of completed objectives compared to the number of non-stretched objectives in this planning interval."
            />
            <TeamPredictabilityRadarChart
              teamPredictabilities={piPredictabilityData?.teamPredictabilities}
              isLoading={isLoadingPiPredictability}
            />
            <ObjectiveStatusChart data={objectiveStatusData} />
            <ObjectiveHealthChart data={objectiveHealthData} />
          </>
        )}
        <LinksCard objectId={planningInterval.id} />
      </Space>
    </>
  )
}

export default PlanningIntervalDetails
