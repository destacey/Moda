'use client'

import { Modal } from 'antd'
import { useEffect, useState } from 'react'
import WorkItemsCumulativeFlowChart from './work-items-cumulative-flow-chart'
import { useGetObjectiveWorkItemMetricsQuery } from '@/src/store/features/planning/planning-interval-api'

export interface WorkItemsDashboardModalProps {
  showDashboard: boolean
  planningIntervalId: string
  objectiveId: string
  onModalClose: () => void
}

const WorkItemsDashboardModal = (props: WorkItemsDashboardModalProps) => {
  const {
    data: workItemsData,
    isLoading,
    isError,
  } = useGetObjectiveWorkItemMetricsQuery({
    planningIntervalId: props.planningIntervalId,
    objectiveId: props.objectiveId,
  })

  useEffect(() => {
    if (isError) {
      console.error('Error fetching objective work item metrics', isError)
    }
  }, [isError])

  return (
    <>
      <Modal
        title="Work Items Dashboard"
        width={'80vw'}
        open={props.showDashboard}
        onCancel={props.onModalClose}
        footer={null}
        okText="Close"
        confirmLoading={isLoading}
        maskClosable={false}
        destroyOnClose={true}
      >
        <WorkItemsCumulativeFlowChart
          workItems={workItemsData}
          isLoading={isLoading}
        />
      </Modal>
    </>
  )
}

export default WorkItemsDashboardModal
