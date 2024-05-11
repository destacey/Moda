'use client'

import { useGetPlanningIntervalObjectiveQuery } from '@/src/store/features/planning/planning-interval-api'
import { Descriptions, Drawer } from 'antd'
import Link from 'next/link'
import ModaMarkdownDescription from '../../components/common/moda-markdown-description'
import dayjs from 'dayjs'
import PlanningIntervalObjectiveWorkItemsCard from '../planning-intervals/[key]/objectives/[objectiveKey]/planning-interval-objective-work-items-card'

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
      width={
        window.innerWidth >= 1500
          ? '30%'
          : window.innerWidth >= 1300
            ? '35%'
            : window.innerWidth >= 1100
              ? '40%'
              : window.innerWidth >= 900
                ? '50%'
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
        canLinkWorkItems={props.canManageObjectives}
      />
    </Drawer>
  )
}

export default PlanningIntervalObjectiveDetailsDrawer
