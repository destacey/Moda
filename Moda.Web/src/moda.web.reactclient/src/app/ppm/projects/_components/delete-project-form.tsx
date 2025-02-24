'use client'

import useAuth from '@/src/components/contexts/auth'
import { ProjectDetailsDto } from '@/src/services/moda-api'
import { useDeleteProjectMutation } from '@/src/store/features/ppm/projects-api'
import { Modal } from 'antd'
import { MessageInstance } from 'antd/es/message/interface'
import { useEffect, useState } from 'react'

export interface DeleteProjectFormProps {
  project: ProjectDetailsDto
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
  messageApi: MessageInstance
}

const DeleteProjectForm = (props: DeleteProjectFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)

  const [deleteProjectMutation, { error: mutationError }] =
    useDeleteProjectMutation()

  const { hasPermissionClaim } = useAuth()
  const canDeleteProject = hasPermissionClaim('Permissions.Projects.Delete')

  const deleteProject = async (project: ProjectDetailsDto) => {
    try {
      const response = await deleteProjectMutation(project.id)

      if (response.error) {
        throw response.error
      }

      return true
    } catch (error) {
      props.messageApi.error(
        error.detail ??
          'An unexpected error occurred while deleting the project.',
      )
      console.log(error)
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      if (await deleteProject(props.project)) {
        // TODO: not working because the parent page is gone
        props.messageApi.success('Successfully deleted Project.')
        props.onFormComplete()
        setIsOpen(false)
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
      props.messageApi.error(
        'An unexpected error occurred while deleting the project.',
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
    if (!props.project) return
    if (canDeleteProject) {
      setIsOpen(props.showForm)
    } else {
      props.onFormCancel()
    }
  }, [canDeleteProject, props])

  return (
    <>
      <Modal
        title="Are you sure you want to delete this Project?"
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
        {props.project?.key} - {props.project?.name}
      </Modal>
    </>
  )
}

export default DeleteProjectForm
