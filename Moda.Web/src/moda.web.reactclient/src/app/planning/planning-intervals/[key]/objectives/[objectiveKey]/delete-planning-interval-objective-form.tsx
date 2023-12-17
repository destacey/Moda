import { getPlanningIntervalsClient } from '@/src/services/clients'
import { PlanningIntervalObjectiveDetailsDto } from '@/src/services/moda-api'
import { Modal, message } from 'antd'
import { useEffect, useState } from 'react'

interface DeletePlanningIntervalObjectiveFormProps {
  showForm: boolean
  objective: PlanningIntervalObjectiveDetailsDto
  onFormSave: () => void
  onFormCancel: () => void
}

const DeletePlanningIntervalObjectiveForm = ({
  showForm,
  objective,
  onFormSave,
  onFormCancel,
}: DeletePlanningIntervalObjectiveFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [messageApi, contextHolder] = message.useMessage()

  const deleteObjective = async (
    objective: PlanningIntervalObjectiveDetailsDto,
  ) => {
    try {
      const planningIntervalsClient = await getPlanningIntervalsClient()
      await planningIntervalsClient.deleteObjective(
        objective.planningInterval.id,
        objective.id,
      )
      return true
    } catch (error) {
      messageApi.error(
        'An unexpected error occurred while deleting the PI objective.',
      )
      console.log(error)
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      if (await deleteObjective(objective)) {
        setIsOpen(false)
        onFormSave()
        messageApi.success('Successfully deleted PI objective.')
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = () => {
    setIsOpen(false)
    onFormCancel()
  }

  useEffect(() => {
    setIsOpen(showForm)
  }, [showForm])

  return (
    <>
      {contextHolder}
      <Modal
        title="Are you sure you want to delete this PI Objective?"
        open={isOpen}
        onOk={handleOk}
        okText="Delete"
        okType="danger"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        maskClosable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnClose={true}
      >
        {objective?.key} - {objective?.name}
      </Modal>
    </>
  )
}

export default DeletePlanningIntervalObjectiveForm
