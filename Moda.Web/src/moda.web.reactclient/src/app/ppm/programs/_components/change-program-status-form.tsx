'use client'

import { useMessage } from '@/src/components/contexts/messaging'
import { useConfirmModal } from '@/src/hooks'
import { ProgramDetailsDto } from '@/src/services/moda-api'
import {
  useActivateProgramMutation,
  useCancelProgramMutation,
  useCompleteProgramMutation,
} from '@/src/store/features/ppm/programs-api'
import { Modal, Space } from 'antd'
import { useCallback } from 'react'

export enum ProgramStatusAction {
  Activate = 'Activate',
  Complete = 'Complete',
  Cancel = 'Cancel',
}

const statusActionToPastTense = (statusAction: ProgramStatusAction) => {
  switch (statusAction) {
    case ProgramStatusAction.Activate:
      return 'activated'
    case ProgramStatusAction.Complete:
      return 'completed'
    case ProgramStatusAction.Cancel:
      return 'cancelled'
    default:
      return statusAction
  }
}

const statusActionToPresentTense = (statusAction: ProgramStatusAction) => {
  switch (statusAction) {
    case ProgramStatusAction.Activate:
      return 'activating'
    case ProgramStatusAction.Complete:
      return 'completing'
    case ProgramStatusAction.Cancel:
      return 'cancelling'
    default:
      return statusAction
  }
}

export interface ChangeProgramStatusFormProps {
  program: ProgramDetailsDto
  statusAction: ProgramStatusAction
  onFormComplete: () => void
  onFormCancel: () => void
}

const ChangeProgramStatusForm = ({
  program,
  statusAction,
  onFormComplete,
  onFormCancel,
}: ChangeProgramStatusFormProps) => {
  const messageApi = useMessage()

  const [activateProgramMutation] = useActivateProgramMutation()
  const [completeProgramMutation] = useCompleteProgramMutation()
  const [cancelProgramMutation] = useCancelProgramMutation()

  const { isOpen, isSaving, handleOk, handleCancel } = useConfirmModal({
    onSubmit: useCallback(async () => {
      // Check start/end dates before activating
      if (
        statusAction === ProgramStatusAction.Activate &&
        (!program.start || !program.end)
      ) {
        messageApi.error(
          'Program start and end dates must be set before activating the program.',
        )
        return false
      }

      try {
        const request = { id: program.id, cacheKey: program.key }
        let response = null
        if (statusAction === ProgramStatusAction.Activate) {
          response = await activateProgramMutation(request)
        } else if (statusAction === ProgramStatusAction.Complete) {
          response = await completeProgramMutation(request)
        } else if (statusAction === ProgramStatusAction.Cancel) {
          response = await cancelProgramMutation(request)
        }

        if (response.error) throw response.error

        messageApi.success(
          `Successfully ${statusActionToPastTense(statusAction)} Program.`,
        )
        return true
      } catch (error) {
        messageApi.error(
          error.detail ??
            `An unexpected error occurred while ${statusActionToPresentTense(statusAction)} the program.`,
        )
        console.log(error)
        return false
      }
    }, [
      program,
      statusAction,
      activateProgramMutation,
      completeProgramMutation,
      cancelProgramMutation,
      messageApi,
    ]),
    onComplete: onFormComplete,
    onCancel: onFormCancel,
    errorMessage: `An unexpected error occurred while ${statusActionToPresentTense(statusAction)} the program.`,
    permission: 'Permissions.Programs.Update',
  })

  return (
    <Modal
      title={`Are you sure you want to ${statusAction} this Program?`}
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
          {program?.key} - {program?.name}
        </div>
        {'This action cannot be undone.'}
      </Space>
    </Modal>
  )
}

export default ChangeProgramStatusForm
