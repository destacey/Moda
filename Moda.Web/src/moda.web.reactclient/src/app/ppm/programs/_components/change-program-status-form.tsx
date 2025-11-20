'use client'

import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { ProgramDetailsDto } from '@/src/services/moda-api'
import {
  useActivateProgramMutation,
  useCancelProgramMutation,
  useCompleteProgramMutation,
} from '@/src/store/features/ppm/programs-api'
import { Modal, Space } from 'antd'
import { useEffect, useState } from 'react'

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
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
}

const ChangeProgramStatusForm = (props: ChangeProgramStatusFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)

  const messageApi = useMessage()

  const [activateProgramMutation, { error: activateError }] =
    useActivateProgramMutation()
  const [completeProgramMutation, { error: closeError }] =
    useCompleteProgramMutation()
  const [cancelProgramMutation, { error: archiveError }] =
    useCancelProgramMutation()

  const { hasPermissionClaim } = useAuth()
  const canUpdateProgram = hasPermissionClaim('Permissions.Programs.Update')

  const changeState = async (
    id: string,
    cacheKey: number,
    statusAction: ProgramStatusAction,
  ) => {
    try {
      const request = { id: id, cacheKey: cacheKey }
      let response = null
      if (statusAction === ProgramStatusAction.Activate) {
        response = await activateProgramMutation(request)
      } else if (statusAction === ProgramStatusAction.Complete) {
        response = await completeProgramMutation(request)
      } else if (statusAction === ProgramStatusAction.Cancel) {
        response = await cancelProgramMutation(request)
      }

      if (response.error) {
        throw response.error
      }

      return true
    } catch (error) {
      messageApi.error(
        error.detail ??
          `An unexpected error occurred while ${statusActionToPresentTense(statusAction)} the program.`,
      )
      console.log(error)
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      if (
        await changeState(
          props.program.id,
          props.program.key,
          props.statusAction,
        )
      ) {
        messageApi.success(
          `Successfully ${statusActionToPastTense(props.statusAction)} Program.`,
        )
        props.onFormComplete()
        setIsOpen(false)
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
      messageApi.error(
        `An unexpected error occurred while ${statusActionToPresentTense(props.statusAction)} the program.`,
      )
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = () => {
    setIsOpen(false)
    props.onFormCancel()
  }

  useEffect(() => {
    if (canUpdateProgram) {
      if (props.program.start && props.program.end) {
        setIsOpen(props.showForm)
      } else {
        messageApi.error(
          'Program start and end dates must be set before activating the program.',
        )
        props.onFormCancel()
      }
    } else {
      messageApi.error('You do not have permission to update programs.')
      props.onFormCancel()
    }
  }, [canUpdateProgram, messageApi, props])

  return (
    <>
      <Modal
        title={`Are you sure you want to ${props.statusAction} this Program?`}
        open={isOpen}
        onOk={handleOk}
        okText={props.statusAction}
        confirmLoading={isSaving}
        onCancel={handleCancel}
        maskClosable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnHidden={true}
      >
        <Space direction="vertical">
          <div>
            {props.program?.key} - {props.program?.name}
          </div>
          {'This action cannot be undone.'}
        </Space>
      </Modal>
    </>
  )
}

export default ChangeProgramStatusForm
