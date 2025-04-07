'use client'

import { MarkdownEditor } from '@/src/components/common/markdown'
import { EmployeeSelect } from '@/src/components/common/organizations'
import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { CreatePortfolioRequest } from '@/src/services/moda-api'
import { useGetEmployeeOptionsQuery } from '@/src/store/features/organizations/employee-api'
import { useCreatePortfolioMutation } from '@/src/store/features/ppm/portfolios-api'
import { toFormErrors } from '@/src/utils'
import { Form, Modal } from 'antd'
import TextArea from 'antd/es/input/TextArea'
import { useCallback, useEffect, useState } from 'react'

const { Item } = Form

export interface CreatePortfolioFormProps {
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
}

interface CreatePortfolioFormValues {
  name: string
  description: string
  sponsorIds: string[]
  ownerIds: string[]
  managerIds: string[]
}

const mapToRequestValues = (
  values: CreatePortfolioFormValues,
): CreatePortfolioRequest => {
  return {
    name: values.name,
    description: values.description,
    sponsorIds: values.sponsorIds,
    ownerIds: values.ownerIds,
    managerIds: values.managerIds,
  } as CreatePortfolioRequest
}

const CreatePortfolioForm = (props: CreatePortfolioFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CreatePortfolioFormValues>()
  const formValues = Form.useWatch([], form)

  const messageApi = useMessage()

  const [createPortfolio, { error: mutationError }] =
    useCreatePortfolioMutation()

  const {
    data: employeeData,
    isLoading: employeeOptionsIsLoading,
    error: employeeOptionsError,
  } = useGetEmployeeOptionsQuery(false)

  const { hasPermissionClaim } = useAuth()
  const canCreatePortfolio = hasPermissionClaim(
    'Permissions.ProjectPortfolios.Create',
  )

  const create = async (values: CreatePortfolioFormValues) => {
    try {
      const request = mapToRequestValues(values)
      const response = await createPortfolio(request)
      if (response.error) {
        throw response.error
      }
      messageApi.success(
        'Portfolio created successfully. Portfolio key: ' + response.data.key,
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
            'An error occurred while creating the portfolio. Please try again.',
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
        'An error occurred while creating the portfolio. Please try again.',
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
    if (canCreatePortfolio) {
      setIsOpen(props.showForm)
    } else {
      props.onFormCancel()
      messageApi.error('You do not have permission to create portfolios.')
    }
  }, [canCreatePortfolio, messageApi, props])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  return (
    <>
      <Modal
        title="Create Portfolio"
        open={isOpen}
        width={'60vw'}
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
          name="create-portfolio-form"
        >
          <Item
            label="Name"
            name="name"
            rules={[
              { required: true, message: 'Name is required' },
              { max: 128 },
            ]}
          >
            <TextArea
              autoSize={{ minRows: 1, maxRows: 2 }}
              showCount
              maxLength={128}
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
            <MarkdownEditor
              value={form.getFieldValue('description')}
              onChange={(value) =>
                form.setFieldValue('description', value || '')
              }
              maxLength={1024}
            />
          </Item>
          <Item name="sponsorIds" label="Sponsors">
            <EmployeeSelect
              employees={employeeData ?? []}
              allowMultiple={true}
              placeholder="Select Sponsors"
            />
          </Item>
          <Item name="ownerIds" label="Owners">
            <EmployeeSelect
              employees={employeeData ?? []}
              allowMultiple={true}
              placeholder="Select Owners"
            />
          </Item>
          <Item name="managerIds" label="Managers">
            <EmployeeSelect
              employees={employeeData ?? []}
              allowMultiple={true}
              placeholder="Select Managers"
            />
          </Item>
        </Form>
      </Modal>
    </>
  )
}

export default CreatePortfolioForm
