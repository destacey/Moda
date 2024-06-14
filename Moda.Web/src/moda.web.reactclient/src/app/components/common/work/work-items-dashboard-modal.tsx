import { Modal, Spin } from 'antd'
import { useState } from 'react'
import WorkItemsCumulativeFlowChart from './work-items-cumulative-flow-chart'
import { useGetObjectiveWorkItemMetricsQuery } from '@/src/store/features/planning/planning-interval-api'

export interface WorkItemsDashboardModalProps {
  showDashboard: boolean
  planningIntervalId: string
  objectiveId: string
  onModalClose: () => void
}

const WorkItemsDashboardModal = (props: WorkItemsDashboardModalProps) => {
  const [open, setOpen] = useState(props.showDashboard)

  const {
    data: workItemsData,
    isLoading,
    isError,
  } = useGetObjectiveWorkItemMetricsQuery({
    planningIntervalId: props.planningIntervalId,
    objectiveId: props.objectiveId,
  })

  const handleClose = () => {
    setOpen(false)
    props.onModalClose()
  }

  return (
    <>
      <Modal
        title="Work Items Dashboard"
        width={'80vw'}
        open={open}
        onCancel={handleClose}
        footer={null}
        okText="Close"
        confirmLoading={isLoading}
        maskClosable={false}
        destroyOnClose={true}
      >
        <Spin spinning={isLoading}>
          <WorkItemsCumulativeFlowChart workItems={workItemsData} />
        </Spin>
      </Modal>
    </>
  )
}

export default WorkItemsDashboardModal
