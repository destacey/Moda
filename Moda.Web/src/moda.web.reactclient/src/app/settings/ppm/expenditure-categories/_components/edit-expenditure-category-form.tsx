'use client'

import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import {
  ExpenditureCategoryDetailsDto,
  UpdateExpenditureCategoryRequest,
} from '@/src/services/moda-api'
import {
  useGetExpenditureCategoryQuery,
  useUpdateExpenditureCategoryMutation,
} from '@/src/store/features/ppm/expenditure-categories-api'
import { toFormErrors } from '@/src/utils'
import { Form, Input, Modal, Switch } from 'antd'
import TextArea from 'antd/es/input/TextArea'
import { useCallback, useEffect, useState } from 'react'

const { Item } = Form

export interface EditExpenditureCategoryFormProps {
  expenditureCategoryId: number
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
}

interface UpdateExpenditureCategoryFormValues {
  name: string
  description: string
  isCapitalizable: boolean
  requiresDepreciation: boolean
  accountingCode?: string
}

const mapToRequestValues = (
  values: UpdateExpenditureCategoryFormValues,
  expenditureCategoryId: number,
): UpdateExpenditureCategoryRequest => {
  return {
    id: expenditureCategoryId,
    name: values.name,
    description: values.description,
    isCapitalizable: values.isCapitalizable,
    requiresDepreciation: values.requiresDepreciation,
    accountingCode: values.accountingCode,
  } as UpdateExpenditureCategoryRequest
}

const EditExpenditureCategoryForm = (
  props: EditExpenditureCategoryFormProps,
) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<UpdateExpenditureCategoryFormValues>()
  const formValues = Form.useWatch([], form)
  const [hideReadOnlyFields, setHideReadOnlyFields] = useState(false)

  const messageApi = useMessage()

  const {
    data: categoryData,
    isLoading,
    error,
  } = useGetExpenditureCategoryQuery(props.expenditureCategoryId)

  const [updateExpenditureCategory, { error: mutationError }] =
    useUpdateExpenditureCategoryMutation()

  const { hasPermissionClaim } = useAuth()
  const canUpdateExpenditureCategory = hasPermissionClaim(
    'Permissions.ExpenditureCategories.Update',
  )

  const mapToFormValues = useCallback(
    (category: ExpenditureCategoryDetailsDto) => {
      if (!category) {
        throw new Error('Expenditure Category not found')
      }
      form.setFieldsValue({
        name: category.name,
        description: category.description,
        isCapitalizable: category.isCapitalizable,
        requiresDepreciation: category.requiresDepreciation,
        accountingCode: category.accountingCode,
      })
    },
    [form],
  )

  const update = async (values: UpdateExpenditureCategoryFormValues) => {
    try {
      const request = mapToRequestValues(values, categoryData.id)
      const response = await updateExpenditureCategory(request)
      if (response.error) {
        throw response.error
      }
      messageApi.success('Expenditure Category updated successfully.')

      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.detail ??
            'An error occurred while updating the expenditure category. Please try again.',
        )
      }

      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      if (await update(values)) {
        setIsOpen(false)
        form.resetFields()
        props.onFormComplete()
      }
    } catch (error) {
      console.error('update error', error)
      messageApi.error(
        'An error occurred while updating the expenditure category. Please try again.',
      )
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = useCallback(() => {
    setIsOpen(false)
    form.resetFields()
    props.onFormCancel()
  }, [form, props])

  useEffect(() => {
    if (!categoryData) return

    if (canUpdateExpenditureCategory) {
      if (categoryData.state.name !== 'Proposed') {
        setHideReadOnlyFields(true)
      }

      setIsOpen(props.showForm)
      if (props.showForm) {
        mapToFormValues(categoryData)
      }
    } else {
      props.onFormCancel()
      messageApi.error(
        'You do not have permission to update expenditure categories.',
      )
    }
  }, [
    canUpdateExpenditureCategory,
    categoryData,
    mapToFormValues,
    messageApi,
    props,
  ])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  useEffect(() => {
    if (error) {
      messageApi.error(
        error.detail ??
          'An error occurred while loading the expenditure category. Please try again.',
      )
    }
  }, [error, messageApi])

  return (
    <>
      <Modal
        title="Edit Expenditure Category"
        open={isOpen}
        onOk={handleOk}
        okButtonProps={{ disabled: !isValid }}
        okText="Save"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        maskClosable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnHidden={true}
      >
        <Form
          form={form}
          size="small"
          layout="vertical"
          name="update-expenditure-category-form"
        >
          <Item
            label="Name"
            name="name"
            rules={[
              { required: true, message: 'Name is required' },
              { max: 64 },
            ]}
          >
            <TextArea
              autoSize={{ minRows: 1, maxRows: 2 }}
              showCount
              maxLength={64}
            />
          </Item>
          <Item
            name="description"
            label="Description"
            rules={[
              { required: true, message: 'Description is required' },
              { max: 1024 },
            ]}
          >
            <TextArea
              autoSize={{ minRows: 6, maxRows: 8 }}
              showCount
              maxLength={1024}
            />
          </Item>
          <Item
            name="isCapitalizable"
            label="Capitalizable?"
            valuePropName="checked"
            hidden={hideReadOnlyFields}
          >
            <Switch checkedChildren="Yes" unCheckedChildren="No" />
          </Item>
          <Item
            name="requiresDepreciation"
            label="Requires Depreciation?"
            valuePropName="checked"
            hidden={hideReadOnlyFields}
          >
            <Switch checkedChildren="Yes" unCheckedChildren="No" />
          </Item>
          <Item
            name="accountingCode"
            label="Accounting Code"
            rules={[{ max: 64 }]}
          >
            <Input showCount maxLength={64} />
          </Item>
        </Form>
      </Modal>
    </>
  )
}

export default EditExpenditureCategoryForm
