'use client'

import useAuth from '@/src/components/contexts/auth'
import { authorizeForm } from '@/src/components/hoc'
import { StrategicInitiativeDetailsDto } from '@/src/services/moda-api'
import { useDeleteStrategicInitiativeMutation } from '@/src/store/features/ppm/strategic-initiatives-api'
import { Modal } from 'antd'
import { MessageInstance } from 'antd/es/message/interface'
import { FC, useEffect, useState } from 'react'

export interface DeleteStrategicInitiativeFormProps {
  strategicInitiative: StrategicInitiativeDetailsDto
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
  messageApi: MessageInstance
}

const DeleteStrategicInitiativeForm = (
  props: DeleteStrategicInitiativeFormProps,
) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)

  const [deleteStrategicInitiativeMutation, { error: mutationError }] =
    useDeleteStrategicInitiativeMutation()

  const deleteStrategicInitiative = async (
    strategicInitiative: StrategicInitiativeDetailsDto,
  ) => {
    try {
      const response = await deleteStrategicInitiativeMutation(
        strategicInitiative.id,
      )

      if (response.error) {
        throw response.error
      }

      return true
    } catch (error) {
      props.messageApi.error(
        error.detail ??
          'An unexpected error occurred while deleting the strategic initiative.',
      )
      console.log(error)
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      if (await deleteStrategicInitiative(props.strategicInitiative)) {
        // TODO: not working because the parent page is gone
        props.messageApi.success('Successfully deleted strategic initiative.')
        props.onFormComplete()
        setIsOpen(false)
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
      props.messageApi.error(
        'An unexpected error occurred while deleting the strategic initiative.',
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
    if (!props.strategicInitiative) return

    setIsOpen(props.showForm)
  }, [props.showForm, props.strategicInitiative])

  return (
    <>
      <Modal
        title="Are you sure you want to delete this Strategic Initiative?"
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
        {props.strategicInitiative?.key} - {props.strategicInitiative?.name}
      </Modal>
    </>
  )
}

const AuthorizedDeleteStrategicInitiativeForm: FC<
  DeleteStrategicInitiativeFormProps
> = (props) => {
  const AuthorizedForm = authorizeForm(
    DeleteStrategicInitiativeForm,
    props.onFormCancel,
    props.messageApi,
    'Permission',
    'Permissions.StrategicInitiatives.Delete',
  )

  return <AuthorizedForm {...props} />
}

export default AuthorizedDeleteStrategicInitiativeForm
