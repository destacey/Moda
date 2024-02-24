'use client'

import {
  PlanningIntervalDetailsDto,
  PlanningIntervalTeamResponse,
} from '@/src/services/moda-api'
import { Col, Flex, Row, Segmented, Space, Tag, Typography } from 'antd'
import { useMemo, useState } from 'react'
import TeamObjectivesListCard from './team-objectives-list-card'
import TeamRisksListCard from './team-risks-list-card'
import Link from 'next/link'
import { BarsOutlined, BuildOutlined } from '@ant-design/icons'
import { SegmentedLabeledOption } from 'antd/es/segmented'
import { PlanningIntervalObjectivesTimeline } from '../../../components'
import {
  useGetPlanningIntervalCalendar,
  useGetPlanningIntervalObjectivesByTeamId,
  useGetPlanningIntervalRisksByTeamId,
  useGetTeamPlanningIntervalPredictability,
} from '@/src/services/queries/planning-queries'

const { Title } = Typography

export interface TeamPlanReviewProps {
  planningInterval: PlanningIntervalDetailsDto
  team: PlanningIntervalTeamResponse
  refreshPlanningInterval: () => void
}

const viewSelectorOptions: SegmentedLabeledOption[] = [
  {
    value: 'List',
    icon: <BarsOutlined alt="List" title="List" />,
  },
  {
    value: 'Timeline',
    icon: <BuildOutlined alt="Timeline" title="Timeline" />,
  },
]

const TeamPlanReview = ({
  planningInterval,
  team,
  refreshPlanningInterval,
}: TeamPlanReviewProps) => {
  const [currentView, setCurrentView] = useState<string | number>('List')

  const calendarQuery = useGetPlanningIntervalCalendar(planningInterval?.id)

  const objectivesQuery = useGetPlanningIntervalObjectivesByTeamId(
    planningInterval?.id,
    team?.id,
  )

  const risksQuery = useGetPlanningIntervalRisksByTeamId(
    planningInterval?.id,
    team?.id,
  )

  const predictabilityQuery = useGetTeamPlanningIntervalPredictability(
    planningInterval?.id,
    team?.id,
  )

  const viewSelector = useMemo(
    () => (
      <Segmented
        options={viewSelectorOptions}
        value={currentView}
        onChange={setCurrentView}
      />
    ),
    [currentView],
  )

  return (
    <>
      <Flex
        justify="space-between"
        align="center"
        style={{ paddingBottom: '16px' }}
      >
        <Space>
          <Title level={3} style={{ margin: '0' }}>
            <Link href={`/organizations/teams/${team?.key}`}>{team?.name}</Link>
          </Title>
          {objectivesQuery?.data?.length > 0 &&
            predictabilityQuery?.data != null && (
              <Tag title="PI Predictability">{`${predictabilityQuery?.data}%`}</Tag>
            )}
        </Space>
        {viewSelector}
      </Flex>
      {currentView === 'List' ? (
        <Row gutter={[16, 16]}>
          <Col xs={24} sm={24} md={24} lg={12}>
            <TeamObjectivesListCard
              objectivesQuery={objectivesQuery}
              teamId={team?.id}
              planningIntervalId={planningInterval?.id}
              newObjectivesAllowed={
                !planningInterval?.objectivesLocked ?? false
              }
              refreshPlanningInterval={refreshPlanningInterval}
            />
          </Col>
          <Col xs={24} sm={24} md={24} lg={12}>
            <TeamRisksListCard riskQuery={risksQuery} teamId={team?.id} />
          </Col>
        </Row>
      ) : (
        <PlanningIntervalObjectivesTimeline
          objectivesQuery={objectivesQuery}
          planningIntervalCalendarQuery={calendarQuery}
        />
      )}
    </>
  )
}

export default TeamPlanReview
