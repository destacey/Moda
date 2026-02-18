'use client'

import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { StrategicInitiativeDetailsDto } from '@/src/services/moda-api'
import {
  useActivateStrategicInitiativeMutation,
  useApproveStrategicInitiativeMutation,
  useCancelStrategicInitiativeMutation,
  useCompleteStrategicInitiativeMutation,
} from '@/src/store/features/ppm/strategic-initiatives-api'
import { Modal, Space } from 'antd'
import { useEffect, useState } from 'react'

export enum StrategicInitiativeStatusAction {
  Approve = 'Approve',
  Activate = 'Activate',
  Complete = 'Complete',
  Cancel = 'Cancel',
}

const statusActionToPastTense = (
  statusAction: StrategicInitiativeStatusAction,
) => {
  switch (statusAction) {
    case StrategicInitiativeStatusAction.Approve:
      return 'approved'
    case StrategicInitiativeStatusAction.Activate:
      return 'activated'
    case StrategicInitiativeStatusAction.Complete:
      return 'completed'
    case StrategicInitiativeStatusAction.Cancel:
      return 'cancelled'
    default:
      return statusAction
  }
}

const statusActionToPresentTense = (
  statusAction: StrategicInitiativeStatusAction,
) => {
  switch (statusAction) {
    case StrategicInitiativeStatusAction.Approve:
      return 'approving'
    case StrategicInitiativeStatusAction.Activate:
      return 'activating'
    case StrategicInitiativeStatusAction.Complete:
      return 'completing'
    case StrategicInitiativeStatusAction.Cancel:
      return 'cancelling'
    default:
      return statusAction
  }
}

export interface ChangeStrategicInitiativeStatusFormProps {
  strategicInitiative: StrategicInitiativeDetailsDto
  statusAction: StrategicInitiativeStatusAction
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
}

const ChangeStrategicInitiativeStatusForm = (
  props: ChangeStrategicInitiativeStatusFormProps,
) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)

  const messageApi = useMessage()

  const [approveStrategicInitiativeMutation, { error: approveError }] =
    useApproveStrategicInitiativeMutation()
  const [activateStrategicInitiativeMutation, { error: activateError }] =
    useActivateStrategicInitiativeMutation()
  const [completeStrategicInitiativeMutation, { error: closeError }] =
    useCompleteStrategicInitiativeMutation()
  const [cancelStrategicInitiativeMutation, { error: archiveError }] =
    useCancelStrategicInitiativeMutation()

  const { hasPermissionClaim } = useAuth()
  const canUpdateStrategicInitiative = hasPermissionClaim(
    'Permissions.StrategicInitiatives.Update',
  )

  const formAction = async (
    id: string,
    cacheKey: number,
    statusAction: StrategicInitiativeStatusAction,
  ) => {
    try {
      const request = { id: id, cacheKey: cacheKey }
      let response = null
      if (statusAction === StrategicInitiativeStatusAction.Approve) {
        response = await approveStrategicInitiativeMutation(request)
      } else if (statusAction === StrategicInitiativeStatusAction.Activate) {
        response = await activateStrategicInitiativeMutation(request)
      } else if (statusAction === StrategicInitiativeStatusAction.Complete) {
        response = await completeStrategicInitiativeMutation(request)
      } else if (statusAction === StrategicInitiativeStatusAction.Cancel) {
        response = await cancelStrategicInitiativeMutation(request)
      }

      if (response.error) {
        throw response.error
      }

      return true
    } catch (error) {
      messageApi.error(
        error.detail ??
          `An unexpected error occurred while ${statusActionToPresentTense(statusAction)} the strategic initiative.`,
      )
      console.log(error)
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      if (
        await formAction(
          props.strategicInitiative.id,
          props.strategicInitiative.key,
          props.statusAction,
        )
      ) {
        messageApi.success(
          `Successfully ${statusActionToPastTense(props.statusAction)} Strategic Initiative.`,
        )
        props.onFormComplete()
        setIsOpen(false)
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
      messageApi.error(
        `An unexpected error occurred while ${statusActionToPresentTense(props.statusAction)} the strategic initiative.`,
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
    if (canUpdateStrategicInitiative) {
      setIsOpen(props.showForm)
    } else {
      messageApi.error(
        'You do not have permission to update strategic initiatives.',
      )
      props.onFormCancel()
    }
  }, [canUpdateStrategicInitiative, messageApi, props])

  return (
    <>
      <Modal
        title={`Are you sure you want to ${props.statusAction} this Strategic Initiative?`}
        open={isOpen}
        onOk={handleOk}
        okText={props.statusAction}
        confirmLoading={isSaving}
        onCancel={handleCancel}
        keyboard={false} // disable esc key to close modal
        destroyOnHidden={true}
      >
        <Space vertical>
          <div>
            {props.strategicInitiative?.key} - {props.strategicInitiative?.name}
          </div>
          {'This action cannot be undone.'}
        </Space>
      </Modal>
    </>
  )
}

export default ChangeStrategicInitiativeStatusForm
