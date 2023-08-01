import { getProgramIncrementsClient } from '@/src/services/clients'
import { ProgramIncrementObjectiveDetailsDto } from '@/src/services/moda-api'
import { Modal, message } from 'antd'
import { useEffect, useState } from 'react'

interface DeleteProgramIncrementObjectiveFormProps {
  showForm: boolean
  objective: ProgramIncrementObjectiveDetailsDto
  onFormSave: () => void
  onFormCancel: () => void
}

const DeleteProgramIncrementObjectiveForm = ({
  showForm,
  objective,
  onFormSave,
  onFormCancel,
}: DeleteProgramIncrementObjectiveFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [messageApi, contextHolder] = message.useMessage()

  const deleteObjective = async (
    objective: ProgramIncrementObjectiveDetailsDto
  ) => {
    try {
      console.log('deleteObjective')
      const programIncrementsClient = await getProgramIncrementsClient()
      await programIncrementsClient.deleteObjective(
        objective.programIncrement.id,
        objective.id
      )
      return true
    } catch (error) {
      messageApi.error(
        'An unexpected error occurred while deleting the PI objective.'
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
        closable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnClose={true}
      >
        {objective?.localId} - {objective?.name}
      </Modal>
    </>
  )
}

export default DeleteProgramIncrementObjectiveForm
