'use client'

import { useGetPlanningIntervalObjectiveQuery } from '@/src/store/features/planning/planning-interval-api'
import { Button, Descriptions, Drawer, Flex } from 'antd'
import Link from 'next/link'
import dayjs from 'dayjs'
import PlanningIntervalObjectiveWorkItemsCard from '../[key]/objectives/[objectiveKey]/planning-interval-objective-work-items-card'
import { getDrawerWidthPercentage } from '@/src/utils/window-utils'
import { MarkdownRenderer } from '@/src/components/common/markdown'
import { FC, useState } from 'react'
import { EditPlanningIntervalObjectiveForm } from '.'

const { Item: DescriptionsItem } = Descriptions

interface PlanningIntervalObjectiveDetailsDrawerProps {
  planningIntervalKey: number
  objectiveKey: number
  drawerOpen: boolean
  onDrawerClose: () => void
  canManageObjectives: boolean
}

const PlanningIntervalObjectiveDetailsDrawer: FC<
  PlanningIntervalObjectiveDetailsDrawerProps
> = (props: PlanningIntervalObjectiveDetailsDrawerProps) => {
  const [openEditObjectiveForm, setOpenEditObjectiveForm] =
    useState<boolean>(false)

  const { data: objectiveData, isLoading: objectiveDataIsLoading } =
    useGetPlanningIntervalObjectiveQuery(
      {
        planningIntervalKey: props.planningIntervalKey.toString(),
        objectiveKey: props.objectiveKey.toString(),
      },
      { skip: !props.planningIntervalKey || !props.objectiveKey },
    )

  if (!props.planningIntervalKey || !props.objectiveKey) {
    return null
  }

  if (!objectiveDataIsLoading && !objectiveData) return null

  return (
    <>
      <Drawer
        title={objectiveData?.name ?? 'Objective'}
        placement="right"
        onClose={props.onDrawerClose}
        open={props.drawerOpen}
        destroyOnHidden={true}
        loading={objectiveDataIsLoading}
        width={getDrawerWidthPercentage()}
        extra={
          props.canManageObjectives && (
            <Button onClick={() => setOpenEditObjectiveForm(true)}>Edit</Button>
          )
        }
      >
        <Flex vertical gap="middle">
          <Flex vertical gap="middle">
            <Descriptions column={1} size="small">
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
            <Descriptions column={1} layout="vertical" size="small">
              <DescriptionsItem label="Description">
                <MarkdownRenderer markdown={objectiveData?.description} />
              </DescriptionsItem>
            </Descriptions>
          </Flex>
          <PlanningIntervalObjectiveWorkItemsCard
            planningIntervalKey={props.planningIntervalKey}
            objectiveKey={props.objectiveKey}
            canLinkWorkItems={props.canManageObjectives}
          />
        </Flex>
      </Drawer>
      {openEditObjectiveForm && (
        <EditPlanningIntervalObjectiveForm
          showForm={openEditObjectiveForm}
          objectiveKey={props.objectiveKey}
          planningIntervalKey={props.planningIntervalKey}
          onFormSave={() => setOpenEditObjectiveForm(false)}
          onFormCancel={() => setOpenEditObjectiveForm(false)}
        />
      )}
    </>
  )
}

export default PlanningIntervalObjectiveDetailsDrawer
