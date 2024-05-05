'use client'

import {
  PlanningIntervalDetailsDto,
  PlanningIntervalTeamResponse,
} from '@/src/services/moda-api'
import {
  Col,
  Descriptions,
  Drawer,
  Flex,
  Row,
  Segmented,
  Space,
  Tag,
  Typography,
} from 'antd'
import { useMemo, useState } from 'react'
import TeamObjectivesListCard from './team-objectives-list-card'
import TeamRisksListCard from './team-risks-list-card'
import Link from 'next/link'
import { BarsOutlined, BuildOutlined } from '@ant-design/icons'
import { SegmentedLabeledOption } from 'antd/es/segmented'
import { PlanningIntervalObjectivesTimeline } from '../../../components'
import {
  useGetPlanningIntervalCalendar,
  useGetPlanningIntervalRisksByTeamId,
  useGetTeamPlanningIntervalPredictability,
} from '@/src/services/queries/planning-queries'
import {
  useGetPlanningIntervalObjectiveQuery,
  useGetPlanningIntervalObjectivesQuery,
} from '@/src/store/features/planning/planning-interval-api'
import ModaMarkdownDescription from '@/src/app/components/common/moda-markdown-description'
import PlanningIntervalObjectiveWorkItemsCard from '../objectives/[objectiveKey]/planning-interval-objective-work-items-card'
import dayjs from 'dayjs'

const { Title } = Typography
const { Item: DescriptionsItem } = Descriptions

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
  const [drawerOpen, setDrawerOpen] = useState(false)
  const [selectedObjectiveId, setSelectedObjectiveId] = useState<string | null>(
    null,
  )

  const calendarQuery = useGetPlanningIntervalCalendar(planningInterval?.id)

  const { data: objectivesData, refetch: refetchObjectives } =
    useGetPlanningIntervalObjectivesQuery(
      {
        planningIntervalId: planningInterval?.id,
        teamId: team?.id,
      },
      { skip: !planningInterval?.id || !team?.id },
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

  const showDrawer = () => {
    setDrawerOpen(true)
  }

  const onDrawerClose = () => {
    setDrawerOpen(false)
    setSelectedObjectiveId(null)
  }

  const onObjectiveClick = (objectiveId: string) => {
    setSelectedObjectiveId(objectiveId)
    showDrawer()
  }

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
          {objectivesData?.length > 0 && predictabilityQuery?.data != null && (
            <Tag title="PI Predictability">{`${predictabilityQuery?.data}%`}</Tag>
          )}
        </Space>
        {viewSelector}
      </Flex>
      {currentView === 'List' ? (
        <Row gutter={[16, 16]}>
          <Col xs={24} sm={24} md={24} lg={12}>
            <TeamObjectivesListCard
              objectivesData={objectivesData}
              refreshObjectives={refetchObjectives}
              teamId={team?.id}
              planningIntervalId={planningInterval?.id}
              newObjectivesAllowed={
                !planningInterval?.objectivesLocked ?? false
              }
              refreshPlanningInterval={refreshPlanningInterval}
              onObjectiveClick={onObjectiveClick}
            />
          </Col>
          <Col xs={24} sm={24} md={24} lg={12}>
            <TeamRisksListCard riskQuery={risksQuery} teamId={team?.id} />
          </Col>
        </Row>
      ) : (
        <PlanningIntervalObjectivesTimeline
          objectivesData={objectivesData}
          planningIntervalCalendarQuery={calendarQuery}
        />
      )}
      {planningInterval?.id && selectedObjectiveId && (
        <ObjectiveDetailsDrawer
          planningIntervalId={planningInterval?.id}
          objectiveId={selectedObjectiveId}
          drawerOpen={drawerOpen}
          onDrawerClose={onDrawerClose}
        />
      )}
    </>
  )
}

interface ObjectiveDetailsDrawerProps {
  planningIntervalId: string
  objectiveId: string
  drawerOpen: boolean
  onDrawerClose: () => void
}

const ObjectiveDetailsDrawer = (props: ObjectiveDetailsDrawerProps) => {
  const { data: objectiveData, isLoading: objectiveDataIsLoading } =
    useGetPlanningIntervalObjectiveQuery(
      {
        planningIntervalId: props.planningIntervalId,
        objectiveId: props.objectiveId,
      },
      { skip: !props.planningIntervalId || !props.objectiveId },
    )

  if (!props.planningIntervalId || !props.objectiveId) {
    return null
  }

  if (!objectiveData) return null

  return (
    <Drawer
      title={objectiveData?.name ?? 'Objective'}
      placement="right"
      onClose={props.onDrawerClose}
      open={props.drawerOpen}
      destroyOnClose={true}
      width={
        window.innerWidth >= 1024
          ? '25%'
          : window.innerWidth >= 768
            ? '33%'
            : '80%'
      }
    >
      <Descriptions column={1}>
        <DescriptionsItem label="Key">
          {objectiveData && (
            <Link
              href={`/planning/planning-intervals/${objectiveData.planningInterval.key}/objectives/${objectiveData.key}`}
            >
              {objectiveData.key}
            </Link>
          )}
        </DescriptionsItem>
        <DescriptionsItem label="Is Stretch?">
          {objectiveData?.isStretch ? 'Yes' : 'No'}
        </DescriptionsItem>
        <DescriptionsItem label="Status">
          {objectiveData?.status.name}
        </DescriptionsItem>
      </Descriptions>
      <Descriptions column={1} layout="vertical">
        <DescriptionsItem label="Description">
          <ModaMarkdownDescription content={objectiveData?.description} />
        </DescriptionsItem>
      </Descriptions>
      <Descriptions column={1}>
        <DescriptionsItem label="Start Date">
          {objectiveData?.startDate &&
            dayjs(objectiveData?.startDate).format('M/D/YYYY')}
        </DescriptionsItem>
        <DescriptionsItem label="Target Date">
          {objectiveData?.targetDate &&
            dayjs(objectiveData?.targetDate).format('M/D/YYYY')}
        </DescriptionsItem>
        {objectiveData?.closedDate && (
          <DescriptionsItem label="Closed Date">
            {dayjs(objectiveData?.closedDate).format('M/D/YYYY')}
          </DescriptionsItem>
        )}
      </Descriptions>
      <PlanningIntervalObjectiveWorkItemsCard
        planningIntervalId={props.planningIntervalId}
        objectiveId={props.objectiveId}
        canLinkWorkItems={false}
      />
    </Drawer>
  )
}

export default TeamPlanReview
