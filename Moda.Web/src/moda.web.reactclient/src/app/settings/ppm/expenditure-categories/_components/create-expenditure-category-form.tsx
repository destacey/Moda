'use client'

import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { CreateExpenditureCategoryRequest } from '@/src/services/moda-api'
import { useCreateExpenditureCategoryMutation } from '@/src/store/features/ppm/expenditure-categories-api'
import { toFormErrors } from '@/src/utils'
import { Form, Input, Modal, Switch } from 'antd'
import TextArea from 'antd/es/input/TextArea'
import { useCallback, useEffect, useState } from 'react'

const { Item } = Form

export interface CreateExpenditureCategoryFormProps {
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
}

interface CreateExpenditureCategoryFormValues {
  name: string
  description: string
  isCapitalizable: boolean
  requiresDepreciation: boolean
  accountingCode?: string
}

const mapToRequestValues = (
  values: CreateExpenditureCategoryFormValues,
): CreateExpenditureCategoryRequest => {
  return {
    name: values.name,
    description: values.description,
    isCapitalizable: values.isCapitalizable,
    requiresDepreciation: values.requiresDepreciation,
    accountingCode: values.accountingCode,
  } as CreateExpenditureCategoryRequest
}

const CreateExpenditureCategoryForm = (
  props: CreateExpenditureCategoryFormProps,
) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CreateExpenditureCategoryFormValues>()
  const formValues = Form.useWatch([], form)

  const messageApi = useMessage()

  const [createExpenditureCategory, { error: mutationError }] =
    useCreateExpenditureCategoryMutation()

  const { hasPermissionClaim } = useAuth()
  const canCreateExpenditureCategory = hasPermissionClaim(
    'Permissions.ExpenditureCategories.Create',
  )

  const create = async (values: CreateExpenditureCategoryFormValues) => {
    try {
      const request = mapToRequestValues(values)
      const response = await createExpenditureCategory(request)
      if (response.error) {
        throw response.error
      }
      messageApi.success(
        'Expenditure Category created successfully. Expenditure Category key: ' +
          response.data,
      )

      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.detail ??
            'An error occurred while creating the expenditure category. Please try again.',
        )
      }
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      if (await create(values)) {
        setIsOpen(false)
        form.resetFields()
        props.onFormComplete()
      }
    } catch (error) {
      console.error('handleOk error', error)
      messageApi.error(
        'An error occurred while creating the expenditure category. Please try again.',
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
    if (canCreateExpenditureCategory) {
      setIsOpen(props.showForm)
    } else {
      props.onFormCancel()
      messageApi.error(
        'You do not have permission to create expenditure categories.',
      )
    }
  }, [canCreateExpenditureCategory, messageApi, props])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  return (
    <>
      <Modal
        title="Create Expenditure Category"
        open={isOpen}
        onOk={handleOk}
        okButtonProps={{ disabled: !isValid }}
        okText="Create"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        maskClosable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnClose={true}
      >
        <Form
          form={form}
          size="small"
          layout="vertical"
          name="create-expenditure-category-form"
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
            initialValue={false}
          >
            <Switch checkedChildren="Yes" unCheckedChildren="No" />
          </Item>
          <Item
            name="requiresDepreciation"
            label="Requires Depreciation?"
            valuePropName="checked"
            initialValue={false}
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

export default CreateExpenditureCategoryForm
