'use client'

import { useMessage } from '@/src/components/contexts/messaging'
import { useConfirmModal } from '@/src/hooks'
import { ProjectDetailsDto } from '@/src/services/moda-api'
import {
  useActivateProjectMutation,
  useCancelProjectMutation,
  useCompleteProjectMutation,
} from '@/src/store/features/ppm/projects-api'
import { Modal, Space } from 'antd'
import { useCallback } from 'react'

export enum ProjectStatusAction {
  Activate = 'Activate',
  Complete = 'Complete',
  Cancel = 'Cancel',
}

const statusActionToPastTense = (statusAction: ProjectStatusAction) => {
  switch (statusAction) {
    case ProjectStatusAction.Activate:
      return 'activated'
    case ProjectStatusAction.Complete:
      return 'completed'
    case ProjectStatusAction.Cancel:
      return 'cancelled'
    default:
      return statusAction
  }
}

const statusActionToPresentTense = (statusAction: ProjectStatusAction) => {
  switch (statusAction) {
    case ProjectStatusAction.Activate:
      return 'activating'
    case ProjectStatusAction.Complete:
      return 'completing'
    case ProjectStatusAction.Cancel:
      return 'cancelling'
    default:
      return statusAction
  }
}

export interface ChangeProjectStatusFormProps {
  project: ProjectDetailsDto
  statusAction: ProjectStatusAction
  onFormComplete: () => void
  onFormCancel: () => void
}

const ChangeProjectStatusForm = ({
  project,
  statusAction,
  onFormComplete,
  onFormCancel,
}: ChangeProjectStatusFormProps) => {
  const messageApi = useMessage()

  const [activateProjectMutation] = useActivateProjectMutation()
  const [completeProjectMutation] = useCompleteProjectMutation()
  const [cancelProjectMutation] = useCancelProjectMutation()

  const { isOpen, isSaving, handleOk, handleCancel } = useConfirmModal({
    onSubmit: useCallback(async () => {
      // Check start/end dates before activating
      if (
        statusAction === ProjectStatusAction.Activate &&
        (!project.start || !project.end)
      ) {
        messageApi.error(
          'Project start and end dates must be set before activating the project.',
        )
        return false
      }

      try {
        const request = { id: project.id, cacheKey: project.key }
        let response = null
        if (statusAction === ProjectStatusAction.Activate) {
          response = await activateProjectMutation(request)
        } else if (statusAction === ProjectStatusAction.Complete) {
          response = await completeProjectMutation(request)
        } else if (statusAction === ProjectStatusAction.Cancel) {
          response = await cancelProjectMutation(request)
        }

        if (response.error) throw response.error

        messageApi.success(
          `Successfully ${statusActionToPastTense(statusAction)} Project.`,
        )
        return true
      } catch (error) {
        messageApi.error(
          error.detail ??
            `An unexpected error occurred while ${statusActionToPresentTense(statusAction)} the project.`,
        )
        console.log(error)
        return false
      }
    }, [
      project,
      statusAction,
      activateProjectMutation,
      completeProjectMutation,
      cancelProjectMutation,
      messageApi,
    ]),
    onComplete: onFormComplete,
    onCancel: onFormCancel,
    errorMessage: `An unexpected error occurred while ${statusActionToPresentTense(statusAction)} the project.`,
    permission: 'Permissions.Projects.Update',
  })

  return (
    <Modal
      title={`Are you sure you want to ${statusAction} this Project?`}
      open={isOpen}
      onOk={handleOk}
      okText={statusAction}
      confirmLoading={isSaving}
      onCancel={handleCancel}
      keyboard={false} // disable esc key to close modal
      destroyOnHidden
    >
      <Space vertical>
        <div>
          {project?.key} - {project?.name}
        </div>
        {'This action cannot be undone.'}
      </Space>
    </Modal>
  )
}

export default ChangeProjectStatusForm
