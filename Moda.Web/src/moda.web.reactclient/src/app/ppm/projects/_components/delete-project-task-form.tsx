'use client'

import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import {
  useDeleteProjectTaskMutation,
  useGetProjectTaskQuery,
} from '@/src/store/features/ppm/project-tasks-api'
import { Modal } from 'antd'
import { useCallback, useEffect, useState } from 'react'

export interface DeleteProjectTaskFormProps {
  projectIdOrKey: string
  taskIdOrKey: string
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
}

const DeleteProjectTaskForm = (props: DeleteProjectTaskFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)

  const messageApi = useMessage()

  const [deleteProjectTaskMutation] = useDeleteProjectTaskMutation()

  const {
    data: taskData,
    isLoading,
    error,
  } = useGetProjectTaskQuery(
    {
      projectIdOrKey: props.projectIdOrKey,
      taskIdOrKey: props.taskIdOrKey,
    },
    { skip: !props.showForm },
  )

  const { hasPermissionClaim } = useAuth()
  const canDeleteTask = hasPermissionClaim('Permissions.Projects.Delete')

  const handleOk = async () => {
    if (!taskData) return

    setIsSaving(true)
    try {
      const response = await deleteProjectTaskMutation({
        projectIdOrKey: props.projectIdOrKey,
        id: taskData.id,
      })

      if (response.error) {
        throw response.error
      }

      messageApi.success('Successfully deleted task.')
      props.onFormComplete()
      setIsOpen(false)
    } catch (error) {
      console.log('handleOk error', error)
      messageApi.error(
        error?.detail ??
          'An unexpected error occurred while deleting the task.',
      )
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = useCallback(() => {
    setIsOpen(false)
    props.onFormCancel()
  }, [props])

  useEffect(() => {
    if (canDeleteTask) {
      setIsOpen(props.showForm)
    } else {
      props.onFormCancel()
      messageApi.error('You do not have permission to delete tasks.')
    }
  }, [canDeleteTask, messageApi, props])

  if (isLoading) {
    return null
  }

  return (
    <>
      <Modal
        title="Are you sure you want to delete this task?"
        open={isOpen}
        onOk={handleOk}
        okText="Delete"
        okType="danger"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        keyboard={false}
        destroyOnHidden={true}
      >
        {taskData?.key} - {taskData?.name}
      </Modal>
    </>
  )
}

export default DeleteProjectTaskForm

