import { useMessage } from '@/src/components/contexts/messaging'
import { authorizeForm } from '@/src/components/hoc'
import { PlanningIntervalObjectiveDetailsDto } from '@/src/services/moda-api'
import { useDeletePlanningIntervalObjectiveMutation } from '@/src/store/features/planning/planning-interval-api'
import { Modal } from 'antd'
import { FC, useEffect, useState } from 'react'

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

  const messageApi = useMessage()

  const [deleteObjective, { error: mutationError }] =
    useDeletePlanningIntervalObjectiveMutation()

  const formAction = async (objective: PlanningIntervalObjectiveDetailsDto) => {
    const response = await deleteObjective({
      planningIntervalId: objective.planningInterval.id,
      planningIntervalKey: objective.planningInterval.key,
      objectiveId: objective.id,
      objectiveKey: objective.key,
      teamId: objective.team.id,
    })

    if (response.error) {
      throw response.error
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      await formAction(objective)
      messageApi.success('Successfully deleted PI objective.')
      setIsOpen(false)
      onFormSave()
    } catch (error) {
      console.log('handleOk error', error)
      messageApi.error(
        error.detail ??
          'An unexpected error occurred while deleting the objective.',
      )
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = () => {
    setIsOpen(false)
    onFormCancel()
  }

  useEffect(() => {
    if (!objective) return

    setIsOpen(showForm)
  }, [objective, showForm])

  return (
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
  )
}

const AuthorizedDeletePlanningIntervalObjectiveForm: FC<
  DeletePlanningIntervalObjectiveFormProps
> = (props) => {
  const AuthorizedForm = authorizeForm(
    DeletePlanningIntervalObjectiveForm,
    props.onFormCancel,
    'Permission',
    'Permissions.StrategicInitiatives.Delete',
  )

  return <AuthorizedForm {...props} />
}

export default AuthorizedDeletePlanningIntervalObjectiveForm
