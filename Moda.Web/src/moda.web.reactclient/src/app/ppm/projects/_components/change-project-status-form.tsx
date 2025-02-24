'use client'

import useAuth from '@/src/components/contexts/auth'
import { ProjectDetailsDto } from '@/src/services/moda-api'
import {
  useActivateProjectMutation,
  useCancelProjectMutation,
  useCompleteProjectMutation,
} from '@/src/store/features/ppm/projects-api'
import { Modal, Space } from 'antd'
import { MessageInstance } from 'antd/es/message/interface'
import { useEffect, useState } from 'react'

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
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
  messageApi: MessageInstance
}

const ChangeProjectStatusForm = (props: ChangeProjectStatusFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)

  const [activateProjectMutation, { error: activateError }] =
    useActivateProjectMutation()
  const [completeProjectMutation, { error: closeError }] =
    useCompleteProjectMutation()
  const [cancelProjectMutation, { error: archiveError }] =
    useCancelProjectMutation()

  const { hasPermissionClaim } = useAuth()
  const canUpdateProject = hasPermissionClaim('Permissions.Projects.Update')

  const changeState = async (
    id: string,
    cacheKey: number,
    statusAction: ProjectStatusAction,
  ) => {
    try {
      const request = { id: id, cacheKey: cacheKey }
      let response = null
      if (statusAction === ProjectStatusAction.Activate) {
        response = await activateProjectMutation(request)
      } else if (statusAction === ProjectStatusAction.Complete) {
        response = await completeProjectMutation(request)
      } else if (statusAction === ProjectStatusAction.Cancel) {
        response = await cancelProjectMutation(request)
      }

      if (response.error) {
        throw response.error
      }

      return true
    } catch (error) {
      props.messageApi.error(
        error.detail ??
          `An unexpected error occurred while ${statusActionToPresentTense(statusAction)} the project.`,
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
          props.project.id,
          props.project.key,
          props.statusAction,
        )
      ) {
        props.messageApi.success(
          `Successfully ${statusActionToPastTense(props.statusAction)} Project.`,
        )
        props.onFormComplete()
        setIsOpen(false)
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
      props.messageApi.error(
        `An unexpected error occurred while ${statusActionToPresentTense(props.statusAction)} the project.`,
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
    if (canUpdateProject) {
      if (props.project.start && props.project.end) {
        setIsOpen(props.showForm)
      } else {
        props.messageApi.error(
          'Project start and end dates must be set before activating the project.',
        )
        props.onFormCancel()
      }
    } else {
      props.messageApi.error('You do not have permission to update projects.')
      props.onFormCancel()
    }
  }, [canUpdateProject, props])

  return (
    <>
      <Modal
        title={`Are you sure you want to ${props.statusAction} this Project?`}
        open={isOpen}
        onOk={handleOk}
        okText={props.statusAction}
        confirmLoading={isSaving}
        onCancel={handleCancel}
        maskClosable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnClose={true}
      >
        <Space direction="vertical">
          <div>
            {props.project?.key} - {props.project?.name}
          </div>
          {'This action cannot be undone.'}
        </Space>
      </Modal>
    </>
  )
}

export default ChangeProjectStatusForm
