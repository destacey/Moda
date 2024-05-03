'use client'

import { WorkItemsListCard } from '@/src/app/components/common/work'
import { FormOutlined } from '@ant-design/icons'
import { Button, Card, Input } from 'antd'
import { useState } from 'react'
import ManagePlanningIntervalObjectiveWorkItemsForm from './manage-planning-interval-objective-work-items-form'
import { useGetObjectiveWorkItemsQuery } from '@/src/store/features/planning/planning-interval-api'

const { Search } = Input

export interface PlanningIntervalObjectiveWorkItemsCardProps {
  planningIntervalId: string
  objectiveId: string
  canLinkWorkItems: boolean
}

const PlanningIntervalObjectiveWorkItemsCard = (
  props: PlanningIntervalObjectiveWorkItemsCardProps,
) => {
  const [openManageWorkItemsForm, setOpenManageWorkItemsForm] =
    useState<boolean>(false)

  const {
    data: workItemsData,
    isLoading,
    isError,
    refetch,
  } = useGetObjectiveWorkItemsQuery({
    planningIntervalId: props.planningIntervalId,
    objectiveId: props.objectiveId,
  })

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
        style={{ width: 400 }}
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
        <WorkItemsListCard workItems={workItemsData} />
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
