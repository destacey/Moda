'use client'

import {
  WorkItemsDashboardModal,
  WorkItemsListCard,
} from '@/src/components/common/work'
import { DashboardOutlined, FormOutlined } from '@ant-design/icons'
import { Button, Card } from 'antd'
import { useState } from 'react'
import ManagePlanningIntervalObjectiveWorkItemsForm from './manage-planning-interval-objective-work-items-form'
import { useGetObjectiveWorkItemsQuery } from '@/src/store/features/planning/planning-interval-api'
import { WorkProgress } from '@/src/components/common'

export interface PlanningIntervalObjectiveWorkItemsCardProps {
  planningIntervalKey: number
  objectiveKey: number
  canLinkWorkItems: boolean
  width?: string | number
}

const PlanningIntervalObjectiveWorkItemsCard = (
  props: PlanningIntervalObjectiveWorkItemsCardProps,
) => {
  const [openWorkItemsDashboard, setOpenWorkItemsDashboard] =
    useState<boolean>(false)
  const [openManageWorkItemsForm, setOpenManageWorkItemsForm] =
    useState<boolean>(false)

  const {
    data: workItemsData,
    isLoading,
    refetch,
  } = useGetObjectiveWorkItemsQuery({
    planningIntervalKey: props.planningIntervalKey.toString(),
    objectiveKey: props.objectiveKey.toString(),
  })

  const onWorkItemsDashboardClosed = () => {
    setOpenWorkItemsDashboard(false)
  }

  const onManageWorkItemsFormClosed = (wasSaved: boolean) => {
    setOpenManageWorkItemsForm(false)
    if (wasSaved) {
      refetch()
    }
  }

  const enableWorkItemsDashboard =
    workItemsData && workItemsData.workItems.length > 0

  return (
    <>
      <Card
        size="small"
        title="Work Items"
        style={{ width: props.width ? props.width : '100%' }}
        // add search from api input
        extra={
          <>
            {enableWorkItemsDashboard && (
              <Button
                type="text"
                icon={<DashboardOutlined />}
                title="Work items dashboard"
                onClick={() => setOpenWorkItemsDashboard(true)}
              />
            )}
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
        {workItemsData &&
          workItemsData.progressSummary &&
          workItemsData.progressSummary.total > 0 && (
            <WorkProgress progress={workItemsData.progressSummary} />
          )}
        <WorkItemsListCard
          workItems={workItemsData?.workItems}
          isLoading={isLoading}
        />
      </Card>
      {openWorkItemsDashboard && (
        <WorkItemsDashboardModal
          showDashboard={openWorkItemsDashboard}
          planningIntervalKey={props.planningIntervalKey}
          objectiveKey={props.objectiveKey}
          onModalClose={() => onWorkItemsDashboardClosed()}
        />
      )}
      {openManageWorkItemsForm && (
        <ManagePlanningIntervalObjectiveWorkItemsForm
          planningIntervalKey={props.planningIntervalKey}
          objectiveKey={props.objectiveKey}
          showForm={openManageWorkItemsForm}
          onFormComplete={() => onManageWorkItemsFormClosed(true)}
          onFormCancel={() => onManageWorkItemsFormClosed(false)}
        />
      )}
    </>
  )
}

export default PlanningIntervalObjectiveWorkItemsCard
