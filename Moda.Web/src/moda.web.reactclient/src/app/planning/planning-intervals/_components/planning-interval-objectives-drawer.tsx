'use client'

import { useGetPlanningIntervalObjectiveQuery } from '@/src/store/features/planning/planning-interval-api'
import { Descriptions, Drawer, Space } from 'antd'
import Link from 'next/link'
import dayjs from 'dayjs'
import PlanningIntervalObjectiveWorkItemsCard from '../[key]/objectives/[objectiveKey]/planning-interval-objective-work-items-card'
import { getDrawerWidthPercentage } from '@/src/utils/window-utils'
import { MarkdownRenderer } from '@/src/components/common/markdown'

const { Item: DescriptionsItem } = Descriptions

interface PlanningIntervalObjectiveDetailsDrawerProps {
  planningIntervalId: string
  objectiveId: string
  drawerOpen: boolean
  onDrawerClose: () => void
  canManageObjectives: boolean
}

const PlanningIntervalObjectiveDetailsDrawer = (
  props: PlanningIntervalObjectiveDetailsDrawerProps,
) => {
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

  if (!objectiveDataIsLoading && !objectiveData) return null

  return (
    <Drawer
      title={objectiveData?.name ?? 'Objective'}
      placement="right"
      onClose={props.onDrawerClose}
      open={props.drawerOpen}
      destroyOnClose={true}
      loading={objectiveDataIsLoading}
      width={getDrawerWidthPercentage()}
    >
      <Space direction="vertical">
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
        <Descriptions column={1} layout="vertical" style={{ paddingTop: 8 }}>
          <DescriptionsItem label="Description">
            <MarkdownRenderer markdown={objectiveData?.description} />
          </DescriptionsItem>
        </Descriptions>
        <Descriptions column={1}>
          <DescriptionsItem label="Start Date">
            {objectiveData?.startDate &&
              dayjs(objectiveData?.startDate).format('MMM D, YYYY')}
          </DescriptionsItem>
          <DescriptionsItem label="Target Date">
            {objectiveData?.targetDate &&
              dayjs(objectiveData?.targetDate).format('MMM D, YYYY')}
          </DescriptionsItem>
          {objectiveData?.closedDate && (
            <DescriptionsItem label="Closed Date">
              {dayjs(objectiveData?.closedDate).format('MMM D, YYYY')}
            </DescriptionsItem>
          )}
        </Descriptions>
        <PlanningIntervalObjectiveWorkItemsCard
          planningIntervalId={props.planningIntervalId}
          objectiveId={props.objectiveId}
          canLinkWorkItems={props.canManageObjectives}
        />
      </Space>
    </Drawer>
  )
}

export default PlanningIntervalObjectiveDetailsDrawer
