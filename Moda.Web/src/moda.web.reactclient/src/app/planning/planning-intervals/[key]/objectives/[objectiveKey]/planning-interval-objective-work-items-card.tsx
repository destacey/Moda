'use client'

import { WorkItemsListCard } from '@/src/app/components/common/work'
import { FormOutlined } from '@ant-design/icons'
import { Button, Card, Input, Progress } from 'antd'
import { useEffect, useState } from 'react'
import ManagePlanningIntervalObjectiveWorkItemsForm from './manage-planning-interval-objective-work-items-form'
import { useGetObjectiveWorkItemsQuery } from '@/src/store/features/planning/planning-interval-api'
import { WorkItemProgressRollupDto } from '@/src/services/moda-api'

const { Search } = Input

export interface PlanningIntervalObjectiveWorkItemsCardProps {
  planningIntervalId: string
  objectiveId: string
  canLinkWorkItems: boolean
  width?: string | number
}

interface ProgressSummary {
  proposed: number
  active: number
  done: number
}
const calculateProgressPercentages = (
  rollup: WorkItemProgressRollupDto,
): ProgressSummary => {
  return {
    proposed: (rollup.proposed / rollup.total) * 100,
    active: (rollup.active / rollup.total) * 100,
    done: (rollup.done / rollup.total) * 100,
  }
}

const PlanningIntervalObjectiveWorkItemsCard = (
  props: PlanningIntervalObjectiveWorkItemsCardProps,
) => {
  const [openManageWorkItemsForm, setOpenManageWorkItemsForm] =
    useState<boolean>(false)
  const [progressSummary, setProgressSummary] =
    useState<ProgressSummary | null>(null)

  const {
    data: workItemsData,
    isLoading,
    isError,
    refetch,
  } = useGetObjectiveWorkItemsQuery({
    planningIntervalId: props.planningIntervalId,
    objectiveId: props.objectiveId,
  })

  useEffect(() => {
    if (workItemsData) {
      setProgressSummary(
        calculateProgressPercentages(workItemsData.progressSummary),
      )
    }
  }, [workItemsData])

  const onManageWorkItemsFormClosed = (wasSaved: boolean) => {
    setOpenManageWorkItemsForm(false)
    if (wasSaved) {
      refetch()
    }
  }

  return (
    <>
      <Card
        size="small"
        title="Work Items"
        style={{ width: props.width ? props.width : '100%' }}
        // add search from api input
        extra={
          <>
            {props.canLinkWorkItems && (
              <Button
                type="text"
                icon={<FormOutlined />}
                title="Manage work items"
                onClick={() => setOpenManageWorkItemsForm(true)}
              />
            )}
          </>
        }
      >
        {progressSummary && (
          <Progress
            percent={progressSummary.done}
            success={{ percent: progressSummary.active }}
          />
        )}
        <WorkItemsListCard
          workItems={workItemsData?.workItems}
          isLoading={isLoading}
        />
      </Card>
      {openManageWorkItemsForm && (
        <ManagePlanningIntervalObjectiveWorkItemsForm
          planningIntervalId={props.planningIntervalId}
          objectiveId={props.objectiveId}
          showForm={openManageWorkItemsForm}
          onFormSave={() => onManageWorkItemsFormClosed(true)}
          onFormCancel={() => onManageWorkItemsFormClosed(false)}
        />
      )}
    </>
  )
}

export default PlanningIntervalObjectiveWorkItemsCard
