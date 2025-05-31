'use client'

import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { ExpenditureCategoryDetailsDto } from '@/src/services/moda-api'
import { useDeleteExpenditureCategoryMutation } from '@/src/store/features/ppm/expenditure-categories-api'
import { Modal } from 'antd'
import { useEffect, useState } from 'react'

export interface DeleteExpenditureCategoryFormProps {
  expenditureCategory: ExpenditureCategoryDetailsDto
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
}

const DeleteExpenditureCategoryForm = (
  props: DeleteExpenditureCategoryFormProps,
) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)

  const messageApi = useMessage()

  const { hasPermissionClaim } = useAuth()
  const canDeleteExpenditureCategory = hasPermissionClaim(
    'Permissions.ExpenditureCategories.Delete',
  )

  const [deleteExpenditureCategory, { error: mutationError }] =
    useDeleteExpenditureCategoryMutation()

  const formAction = async (category: ExpenditureCategoryDetailsDto) => {
    try {
      const response = await deleteExpenditureCategory(category.id)

      if (response.error) {
        throw response.error
      }

      return true
    } catch (error) {
      messageApi.error(
        error.detail ??
          'An unexpected error occurred while deleting the expenditure category.',
      )
      console.log(error)
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      if (await formAction(props.expenditureCategory)) {
        // TODO: not working because the parent page is gone
        messageApi.success('Successfully deleted expenditure category.')
        props.onFormComplete()
        setIsOpen(false)
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
      messageApi.error(
        'An unexpected error occurred while deleting the expenditure category.',
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
    if (!props.expenditureCategory) return
    if (canDeleteExpenditureCategory) {
      setIsOpen(props.showForm)
    } else {
      props.onFormCancel()
    }
  }, [canDeleteExpenditureCategory, props])

  return (
    <>
      <Modal
        title="Are you sure you want to delete this Expenditure Category?"
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
        {props.expenditureCategory?.id} - {props.expenditureCategory?.name}
      </Modal>
    </>
  )
}

export default DeleteExpenditureCategoryForm
