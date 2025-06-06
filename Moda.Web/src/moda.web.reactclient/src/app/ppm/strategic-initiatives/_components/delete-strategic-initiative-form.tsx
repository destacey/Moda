'use client'

import { useMessage } from '@/src/components/contexts/messaging'
import { authorizeForm } from '@/src/components/hoc'
import { StrategicInitiativeDetailsDto } from '@/src/services/moda-api'
import { useDeleteStrategicInitiativeMutation } from '@/src/store/features/ppm/strategic-initiatives-api'
import { Modal } from 'antd'
import { FC, useEffect, useState } from 'react'

export interface DeleteStrategicInitiativeFormProps {
  strategicInitiative: StrategicInitiativeDetailsDto
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
}

const DeleteStrategicInitiativeForm = (
  props: DeleteStrategicInitiativeFormProps,
) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)

  const messageApi = useMessage()

  const [deleteStrategicInitiativeMutation, { error: mutationError }] =
    useDeleteStrategicInitiativeMutation()

  const formAction = async (
    strategicInitiative: StrategicInitiativeDetailsDto,
  ) => {
    const response = await deleteStrategicInitiativeMutation(
      strategicInitiative.id,
    )

    if (response.error) {
      throw response.error
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      await formAction(props.strategicInitiative)

      messageApi.success('Successfully deleted strategic initiative.')
      setIsOpen(false)
      props.onFormComplete()
    } catch (error) {
      console.log('handleOk error', error)
      messageApi.error(
        error.detail ??
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
        destroyOnHidden={true}
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
    'Permission',
    'Permissions.StrategicInitiatives.Delete',
  )

  return <AuthorizedForm {...props} />
}

export default AuthorizedDeleteStrategicInitiativeForm
