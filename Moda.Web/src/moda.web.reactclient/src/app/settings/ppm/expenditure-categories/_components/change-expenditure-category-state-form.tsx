'use client'

import useAuth from '@/src/components/contexts/auth'
import { ExpenditureCategoryDetailsDto } from '@/src/services/moda-api'
import {
  useActivateExpenditureCategoryMutation,
  useArchiveExpenditureCategoryMutation,
} from '@/src/store/features/ppm/expenditure-categories-api'
import { Modal, Space } from 'antd'
import { MessageInstance } from 'antd/es/message/interface'
import { useEffect, useState } from 'react'

export enum ExpenditureCategoryStateAction {
  Activate = 'Activate',
  Archive = 'Archive',
}

export interface ChangeExpenditureCategoryStateFormProps {
  expenditureCategory: ExpenditureCategoryDetailsDto
  stateAction: ExpenditureCategoryStateAction
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
  messageApi: MessageInstance
}

const ChangeExpenditureCategoryStateForm = (
  props: ChangeExpenditureCategoryStateFormProps,
) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)

  const [activateExpenditureCategoryMutation, { error: activateError }] =
    useActivateExpenditureCategoryMutation()
  const [archiveExpenditureCategoryMutation, { error: archiveError }] =
    useArchiveExpenditureCategoryMutation()

  const { hasPermissionClaim } = useAuth()
  const canUpdateExpenditureCategory = hasPermissionClaim(
    'Permissions.ExpenditureCategories.Update',
  )

  const changeState = async (
    id: number,
    stateAction: ExpenditureCategoryStateAction,
  ) => {
    try {
      let response = null
      if (stateAction === ExpenditureCategoryStateAction.Activate) {
        response = await activateExpenditureCategoryMutation(id)
      } else if (stateAction === ExpenditureCategoryStateAction.Archive) {
        response = await archiveExpenditureCategoryMutation(id)
      }

      if (response.error) {
        throw response.error
      }

      return true
    } catch (error) {
      props.messageApi.error(
        error.detail ??
          `An unexpected error occurred while ${stateAction}ing the expenditure category.`,
      )
      console.log(error)
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      if (await changeState(props.expenditureCategory.id, props.stateAction)) {
        props.messageApi.success(
          `Successfully ${props.stateAction}d expenditure category.`,
        )
        props.onFormComplete()
        setIsOpen(false)
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
      props.messageApi.error(
        `An unexpected error occurred while ${props.stateAction}ing the expenditure category.`,
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
    if (canUpdateExpenditureCategory) {
      setIsOpen(props.showForm)
    } else {
      props.onFormCancel()
    }
  }, [canUpdateExpenditureCategory, props])

  return (
    <>
      <Modal
        title={`Are you sure you want to ${props.stateAction} this Expenditure Category?`}
        open={isOpen}
        onOk={handleOk}
        okText={props.stateAction}
        confirmLoading={isSaving}
        onCancel={handleCancel}
        maskClosable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnClose={true}
      >
        <Space direction="vertical">
          <div>
            {props.expenditureCategory?.id} - {props.expenditureCategory?.name}
          </div>
          {'This action cannot be undone.'}
        </Space>
      </Modal>
    </>
  )
}

export default ChangeExpenditureCategoryStateForm
