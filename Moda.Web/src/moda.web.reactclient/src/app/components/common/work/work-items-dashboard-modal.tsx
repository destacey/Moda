import { Modal, Spin } from 'antd'
import { useState } from 'react'
import WorkItemsCumulativeFlowChart from './work-items-cumulative-flow-chart'

export interface WorkItemsDashboardModalProps {
  showDashboard: boolean
  isLoading: boolean
  onModalClose: () => void
}

const WorkItemsDashboardModal = ({
  showDashboard,
  isLoading,
  onModalClose,
}: WorkItemsDashboardModalProps) => {
  const [open, setOpen] = useState(showDashboard)

  const handleClose = () => {
    setOpen(false)
    onModalClose()
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
          <WorkItemsCumulativeFlowChart isLoading={isLoading} />
        </Spin>
      </Modal>
    </>
  )
}

export default WorkItemsDashboardModal
