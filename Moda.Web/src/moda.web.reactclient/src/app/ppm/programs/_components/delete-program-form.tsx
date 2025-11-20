'use client'

import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { ProgramDetailsDto } from '@/src/services/moda-api'
import { useDeleteProgramMutation } from '@/src/store/features/ppm/programs-api'
import { Modal } from 'antd'
import { useEffect, useState } from 'react'

export interface DeleteProgramFormProps {
  program: ProgramDetailsDto
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
}

const DeleteProgramForm = (props: DeleteProgramFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)

  const messageApi = useMessage()

  const [deleteProgramMutation, { error: mutationError }] =
    useDeleteProgramMutation()

  const { hasPermissionClaim } = useAuth()
  const canDeleteProgram = hasPermissionClaim('Permissions.Programs.Delete')

  const formAction = async (program: ProgramDetailsDto) => {
    try {
      const response = await deleteProgramMutation(program.id)

      if (response.error) {
        throw response.error
      }

      return true
    } catch (error) {
      messageApi.error(
        error.detail ??
          'An unexpected error occurred while deleting the program.',
      )
      console.log(error)
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      if (await formAction(props.program)) {
        messageApi.success('Successfully deleted Program.')
        props.onFormComplete()
        setIsOpen(false)
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
      messageApi.error(
        'An unexpected error occurred while deleting the program.',
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
    if (!props.program) return
    if (canDeleteProgram) {
      setIsOpen(props.showForm)
    } else {
      props.onFormCancel()
    }
  }, [canDeleteProgram, props])

  return (
    <>
      <Modal
        title="Are you sure you want to delete this Program?"
        open={isOpen}
        onOk={handleOk}
        okText="Delete"
        okType="danger"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        maskClosable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnHidden={true}
      >
        {props.program?.key} - {props.program?.name}
      </Modal>
    </>
  )
}

export default DeleteProgramForm
